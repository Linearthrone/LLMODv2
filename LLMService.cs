using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace LLMOverlay
{
    public class LLMService
    {
        private readonly HttpClient _httpClient;
        private string _currentModel = "gpt-3.5-turbo";
        private string _apiKey = string.Empty;
        private string _endpoint = "https://api.openai.com/v1";
        private double _temperature = 0.7;
        private readonly List<ChatMessage> _conversationHistory;

        public LLMService()
        {
            _httpClient = new HttpClient();
            _conversationHistory = new List<ChatMessage>();
            LoadSettings();
        }

        public void SetModel(string model)
        {
            _currentModel = model;
            
            // Adjust settings based on model
            switch (model)
            {
                case "gpt-3.5-turbo":
                    _endpoint = "https://api.openai.com/v1";
                    break;
                case "gpt-4":
                    _endpoint = "https://api.openai.com/v1";
                    break;
                case "claude-3":
                    _endpoint = "https://api.anthropic.com/v1";
                    break;
                case "local":
                    _endpoint = "http://localhost:1234/v1";
                    break;
            }
        }

        public async Task<string> SendMessageAsync(string message, List<MediaAttachment> attachments = null)
        {
            try
            {
                // Add message to conversation history
                _conversationHistory.Add(new ChatMessage
                {
                    Sender = "user",
                    Content = message,
                    Timestamp = DateTime.Now.ToString("HH:mm")
                });

                // Process attachments if any
                string processedMessage = await ProcessAttachments(message, attachments);

                // Send request based on current model
                string response = _currentModel switch
                {
                    "gpt-3.5-turbo" or "gpt-4" or "local" => await SendOpenAIRequest(processedMessage),
                    "claude-3" => await SendClaudeRequest(processedMessage),
                    _ => "Unsupported model selected"
                };

                // Add response to conversation history
                _conversationHistory.Add(new ChatMessage
                {
                    Sender = "assistant",
                    Content = response,
                    Timestamp = DateTime.Now.ToString("HH:mm")
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

        private async Task<string> ProcessAttachments(string message, List<MediaAttachment> attachments)
        {
            if (attachments == null || attachments.Count == 0)
                return message;

            var processedBuilder = new StringBuilder(message);
            processedBuilder.AppendLine();

            foreach (var attachment in attachments)
            {
                try
                {
                    if (IsImageFile(attachment.FileType))
                    {
                        processedBuilder.AppendLine($"[Image: {attachment.FileName}]");
                    }
                    else if (attachment.FileType.ToLower() == ".txt")
                    {
                        // Read text file content
                        var file = await StorageFile.GetFileFromPathAsync(attachment.FilePath);
                        var content = await FileIO.ReadTextAsync(file);
                        processedBuilder.AppendLine();
                        processedBuilder.AppendLine($"--- Content of {attachment.FileName} ---");
                        processedBuilder.AppendLine(content);
                        processedBuilder.AppendLine("--- End of file content ---");
                    }
                    else if (attachment.FileType.ToLower() == ".pdf")
                    {
                        processedBuilder.AppendLine($"[PDF Document: {attachment.FileName}]");
                        // Note: PDF processing would require additional libraries
                    }
                    else
                    {
                        processedBuilder.AppendLine($"[File: {attachment.FileName}]");
                    }
                }
                catch (Exception ex)
                {
                    processedBuilder.AppendLine($"[Error processing {attachment.FileName}: {ex.Message}]");
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

        #region Settings Management

        private async void LoadSettings()
        {
            try
            {
                var localSettings = ApplicationData.Current.LocalSettings;
                
                if (localSettings.Values.TryGetValue("LLM_ApiKey", out var apiKey))
                    _apiKey = apiKey.ToString();
                
                if (localSettings.Values.TryGetValue("LLM_Endpoint", out var endpoint))
                    _endpoint = endpoint.ToString();
                
                if (localSettings.Values.TryGetValue("LLM_Temperature", out var temperature))
                    _temperature = Convert.ToDouble(temperature);
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
                var localSettings = ApplicationData.Current.LocalSettings;
                localSettings.Values["LLM_ApiKey"] = _apiKey;
                localSettings.Values["LLM_Endpoint"] = _endpoint;
                localSettings.Values["LLM_Temperature"] = _temperature;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to save settings: {ex.Message}");
            }
        }

        public string GetApiKey() => _apiKey;
        public string GetEndpoint() => _endpoint;
        public double GetTemperature() => _temperature;

        public void SetApiKey(string apiKey) => _apiKey = apiKey;
        public void SetEndpoint(string endpoint) => _endpoint = endpoint;
        public void SetTemperature(double temperature) => _temperature = temperature;

        #endregion

        #region Response Models

        private class OpenAIResponse
        {
            public List<OpenAIChoice> Choices { get; set; } = new List<OpenAIChoice>();
        }

        private class OpenAIChoice
        {
            public OpenAIMessage Message { get; set; } = new OpenAIMessage();
        }

        private class OpenAIMessage
        {
            public string Content { get; set; } = string.Empty;
        }

        private class ClaudeResponse
        {
            public List<ClaudeContent> Content { get; set; } = new List<ClaudeContent>();
        }

        private class ClaudeContent
        {
            public string Text { get; set; } = string.Empty;
        }

        #endregion
    }

    // Extension method for Contains in LLMService
    public static class StringExtensions
    {
        public static bool Contains(this string source, string value, StringComparison comparisonType)
        {
            return source?.IndexOf(value, comparisonType) >= 0;
        }
    }
}