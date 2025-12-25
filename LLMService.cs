using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace LLMOverlay
{
    public class ChatMessage
    {
        public string Sender { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }

    public class MediaAttachment
    {
        public string FilePath { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
    }

    public class LLMService
    {
        private readonly HttpClient _httpClient;
        private string _currentModel = "gpt-3.5-turbo";
        private string _apiKey = string.Empty;
        private string _endpoint = "https://api.openai.com/v1";
        private double _temperature = 0.7;
        private readonly List<ChatMessage> _conversationHistory;
        private readonly List<MediaAttachment> _mediaAttachments;

        public LLMService()
        {
            _httpClient = new HttpClient();
            _conversationHistory = new List<ChatMessage>();
            _mediaAttachments = new List<MediaAttachment>();
            LoadSettings();
        }

        public void SetModel(string model)
        {
            _currentModel = model;
            
            // Adjust settings based on model
            switch (model)
            {
                case "gpt-3.5-turbo":
                case "gpt-4":
                    _endpoint = "https://api.openai.com/v1";
                    break;
                case "claude-3":
                    _endpoint = "https://api.anthropic.com/v1";
                    break;
                case "local":
                    _endpoint = "http://localhost:1234/v1";
                    break;
                case "ollama":
                    _endpoint = "http://localhost:11434/v1";
                    break;
                case "huggingface":
                    _endpoint = "https://api-inference.huggingface.co";
                    break;
            }
        }

        public async Task<string> SendMessageAsync(string message, string attachments = null)
        {
            try
            {
                // Add message to conversation history
                _conversationHistory.Add(new ChatMessage
                {
                    Sender = "user",
                    Content = message,
                    Timestamp = DateTime.Now
                });

                // Process attachments if any
                string processedMessage = await ProcessAttachments(message, attachments);

                // Send request based on current model
                string response = _currentModel switch
                {
                    "gpt-3.5-turbo" or "gpt-4" or "local" or "ollama" => await SendOpenAIRequest(processedMessage),
                    "claude-3" => await SendClaudeRequest(processedMessage),
                    "huggingface" => await SendHuggingFaceRequest(processedMessage),
                    _ => "Unsupported model selected"
                };

                // Add response to conversation history
                _conversationHistory.Add(new ChatMessage
                {
                    Sender = "assistant",
                    Content = response,
                    Timestamp = DateTime.Now
                });

                // Keep only last 10 messages in history
                if (_conversationHistory.Count > 20)
                {
                    _conversationHistory.RemoveRange(0, _conversationHistory.Count - 20);
                }

                return response;
            }
            catch (Exception ex)
            {
                return $"Error communicating with LLM: {ex.Message}";
            }
        }

        private async Task<string> ProcessAttachments(string message, string attachmentPaths)
        {
            if (string.IsNullOrEmpty(attachmentPaths))
                return message;

            var attachmentList = attachmentPaths.Split(',').Select(p => p.Trim()).ToList();
            var processedBuilder = new StringBuilder(message);
            processedBuilder.AppendLine();

            foreach (var attachmentPath in attachmentList)
            {
                try
                {
                    string fileType = Path.GetExtension(attachmentPath);
                    string fileName = Path.GetFileName(attachmentPath);

                    if (IsImageFile(fileType))
                    {
                        processedBuilder.AppendLine($"[Image: {fileName}]");
                    }
                    else if (fileType.Equals(".txt", StringComparison.OrdinalIgnoreCase))
                    {
                        // Read text file content
                        if (File.Exists(attachmentPath))
                        {
                            var content = await File.ReadAllTextAsync(attachmentPath);
                            processedBuilder.AppendLine();
                            processedBuilder.AppendLine($"--- Content of {fileName} ---");
                            processedBuilder.AppendLine(content);
                            processedBuilder.AppendLine("--- End of file content ---");
                        }
                    }
                    else if (fileType.Equals(".pdf", StringComparison.OrdinalIgnoreCase))
                    {
                        processedBuilder.AppendLine($"[PDF Document: {fileName}]");
                        // Note: PDF processing would require additional libraries
                    }
                    else
                    {
                        processedBuilder.AppendLine($"[File: {fileName}]");
                    }
                }
                catch (Exception ex)
                {
                    processedBuilder.AppendLine($"[Error processing attachment: {ex.Message}]");
                }
            }

            return processedBuilder.ToString();
        }

        private bool IsImageFile(string fileType)
        {
            var imageTypes = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
            return imageTypes.Contains(fileType.ToLower());
        }

        private async Task<string> SendOpenAIRequest(string message)
        {
            if (string.IsNullOrEmpty(_apiKey))
            {
                throw new InvalidOperationException("API key is required for OpenAI models");
            }

            var requestBody = new
            {
                model = _currentModel,
                messages = _conversationHistory.Select(m => new
                {
                    role = m.Sender == "user" ? "user" : "assistant",
                    content = m.Content
                }).ToList(),
                temperature = _temperature,
                max_tokens = 2000,
                stream = false
            };

            var json = JsonConvert.SerializeObject(requestBody, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });

            using var request = new HttpRequestMessage(HttpMethod.Post, $"{_endpoint}/chat/completions")
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"API Error: {response.StatusCode} - {responseContent}");
            }

            var result = JsonConvert.DeserializeObject<OpenAIResponse>(responseContent);
            return result?.Choices?.FirstOrDefault()?.Message?.Content ?? "No response received";
        }

        private async Task<string> SendClaudeRequest(string message)
        {
            if (string.IsNullOrEmpty(_apiKey))
            {
                throw new InvalidOperationException("API key is required for Claude models");
            }

            var requestBody = new
            {
                model = "claude-3-sonnet-20240229",
                max_tokens = 2000,
                messages = _conversationHistory.Select(m => new
                {
                    role = m.Sender == "user" ? "user" : "assistant",
                    content = m.Content
                }).ToList()
            };

            var json = JsonConvert.SerializeObject(requestBody, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });

            using var request = new HttpRequestMessage(HttpMethod.Post, $"{_endpoint}/messages")
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            request.Headers.Add("anthropic-version", "2023-06-01");

            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"API Error: {response.StatusCode} - {responseContent}");
            }

            var result = JsonConvert.DeserializeObject<ClaudeResponse>(responseContent);
            return result?.Content?.FirstOrDefault()?.Text ?? "No response received";
        }

        private async Task<string> SendHuggingFaceRequest(string message)
        {
            if (string.IsNullOrEmpty(_apiKey))
            {
                throw new InvalidOperationException("API key is required for HuggingFace models");
            }

            var requestBody = new
            {
                model = _currentModel,
                messages = _conversationHistory.Select(m => new
                {
                    role = m.Sender == "user" ? "user" : "assistant",
                    content = m.Content
                }).ToList(),
                temperature = _temperature
            };

            var json = JsonConvert.SerializeObject(requestBody, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });

            using var request = new HttpRequestMessage(HttpMethod.Post, $"{_endpoint}/chat/completions")
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"API Error: {response.StatusCode} - {responseContent}");
            }

            var result = JsonConvert.DeserializeObject<OpenAIResponse>(responseContent);
            return result?.Choices?.FirstOrDefault()?.Message?.Content ?? "No response received";
        }

        #region Settings Management

        private async void LoadSettings()
        {
            try
            {
                var settingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LLMOverlay", "llm_settings.json");
                
                if (File.Exists(settingsPath))
                {
                    var settingsJson = await File.ReadAllTextAsync(settingsPath);
                    var settings = JsonConvert.DeserializeObject<Dictionary<string, object>>(settingsJson);
                    
                    if (settings != null)
                    {
                        if (settings.ContainsKey("LLM_ApiKey") && settings["LLM_ApiKey"] != null)
                            _apiKey = settings["LLM_ApiKey"]!.ToString() ?? string.Empty;
                        
                        if (settings.ContainsKey("LLM_Endpoint") && settings["LLM_Endpoint"] != null)
                            _endpoint = settings["LLM_Endpoint"]!.ToString() ?? "https://api.openai.com/v1";
                        
                        if (settings.ContainsKey("LLM_Temperature") && settings["LLM_Temperature"] != null)
                            _temperature = Convert.ToDouble(settings["LLM_Temperature"]);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading settings: {ex.Message}");
            }
        }

        public async Task SaveSettings()
        {
            try
            {
                var settings = new Dictionary<string, object>
                {
                    ["LLM_ApiKey"] = _apiKey,
                    ["LLM_Endpoint"] = _endpoint,
                    ["LLM_Temperature"] = _temperature
                };

                var settingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LLMOverlay");
                Directory.CreateDirectory(settingsPath);
                
                var settingsFile = Path.Combine(settingsPath, "llm_settings.json");
                var settingsJson = JsonConvert.SerializeObject(settings, Formatting.Indented);
                
                await File.WriteAllTextAsync(settingsFile, settingsJson);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to save settings: {ex.Message}");
            }
        }

        public void UpdateSettings(Dictionary<string, string> settings)
        {
            try
            {
                if (settings.ContainsKey("ApiKey") && !string.IsNullOrEmpty(settings["ApiKey"]))
                    _apiKey = settings["ApiKey"];
                
                if (settings.ContainsKey("Endpoint") && !string.IsNullOrEmpty(settings["Endpoint"]))
                    _endpoint = settings["Endpoint"];
                
                if (settings.ContainsKey("Model") && !string.IsNullOrEmpty(settings["Model"]))
                    SetModel(settings["Model"]);
                
                // Save the updated settings
                _ = SaveSettings();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating settings: {ex.Message}");
            }
        }

        public string GetApiKey() => _apiKey;
        public string GetEndpoint() => _endpoint;
        public double GetTemperature() => _temperature;
        public string GetCurrentModel() => _currentModel;

        public void SetApiKey(string apiKey) => _apiKey = apiKey;
        public void SetEndpoint(string endpoint) => _endpoint = endpoint;
        public void SetTemperature(double temperature) => _temperature = temperature;

        #endregion

        #region Response Models

        private class OpenAIResponse
        {
            public List<OpenAIChoice>? Choices { get; set; }
        }

        private class OpenAIChoice
        {
            public OpenAIMessage? Message { get; set; }
        }

        private class OpenAIMessage
        {
            public string? Content { get; set; }
        }

        private class ClaudeResponse
        {
            public List<ClaudeContent>? Content { get; set; }
        }

        private class ClaudeContent
        {
            public string? Text { get; set; }
        }

        #endregion
    }

    // Extension method for Contains
    public static class StringExtensions
    {
        public static bool Contains(this string? source, string value, StringComparison comparisonType)
        {
            return source?.IndexOf(value, comparisonType) >= 0;
        }
    }
}
