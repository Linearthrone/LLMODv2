using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using Newtonsoft.Json;
using System.Diagnostics;
using System.IO;

namespace LLMOverlay
{
    public partial class LoadModelWindow : Window
    {
        private Dictionary<string, object> _modelParameters;
        private readonly HttpClient _httpClient;

        public LoadModelWindow()
        {
            InitializeComponent();
            _modelParameters = new Dictionary<string, object>();
            _httpClient = new HttpClient();
            InitializeDefaults();
        }

        private void InitializeDefaults()
        {
            // Set default values
            ApiTypeComboBox.SelectedIndex = 0; // OpenAI
            EnableStreamingCheckBox.IsChecked = true;
            UpdateLocalControlsVisibility();
            
            // Initialize parameter displays
            UpdateParameterDisplays();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void ApiTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateApiConfiguration();
            PopulateModels();
            UpdateLocalControlsVisibility();
        }

        private void UpdateApiConfiguration()
        {
            var selectedApi = (ApiTypeComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString();
            
            switch (selectedApi)
            {
                case "OpenAI (ChatGPT)":
                    EndpointTextBox.Text = "https://api.openai.com/v1";
                    break;
                case "Anthropic (Claude)":
                    EndpointTextBox.Text = "https://api.anthropic.com/v1";
                    break;
                case "Google AI (Gemini)":
                    EndpointTextBox.Text = "https://generativelanguage.googleapis.com/v1";
                    break;
                case "Mistral AI":
                    EndpointTextBox.Text = "https://api.mistral.ai/v1";
                    break;
                case "OpenRouter":
                    EndpointTextBox.Text = "https://openrouter.ai/api/v1";
                    break;
                case "Local API (Ollama)":
                    EndpointTextBox.Text = "http://localhost:11434/v1";
                    break;
                case "Local API (KoboldCpp)":
                    EndpointTextBox.Text = "http://localhost:5001/v1";
                    break;
                case "Custom API":
                    EndpointTextBox.Text = "";
                    break;
            }
        }

        private void PopulateModels()
        {
            ModelComboBox.Items.Clear();
            
            var selectedApi = (ApiTypeComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString();
            
            switch (selectedApi)
            {
                case "OpenAI (ChatGPT)":
                    ModelComboBox.Items.Add("gpt-4-turbo-preview");
                    ModelComboBox.Items.Add("gpt-4");
                    ModelComboBox.Items.Add("gpt-3.5-turbo");
                    ModelComboBox.Items.Add("gpt-3.5-turbo-16k");
                    break;
                case "Anthropic (Claude)":
                    ModelComboBox.Items.Add("claude-3-opus-20240229");
                    ModelComboBox.Items.Add("claude-3-sonnet-20240229");
                    ModelComboBox.Items.Add("claude-3-haiku-20240307");
                    ModelComboBox.Items.Add("claude-2.1");
                    ModelComboBox.Items.Add("claude-2.0");
                    break;
                case "Google AI (Gemini)":
                    ModelComboBox.Items.Add("gemini-pro");
                    ModelComboBox.Items.Add("gemini-pro-vision");
                    break;
                case "Mistral AI":
                    ModelComboBox.Items.Add("mistral-large-latest");
                    ModelComboBox.Items.Add("mistral-medium-latest");
                    ModelComboBox.Items.Add("mistral-small-latest");
                    ModelComboBox.Items.Add("mistral-7b");
                    break;
                case "OpenRouter":
                    ModelComboBox.Items.Add("openai/gpt-4-turbo-preview");
                    ModelComboBox.Items.Add("anthropic/claude-3-opus");
                    ModelComboBox.Items.Add("google/gemini-pro");
                    ModelComboBox.Items.Add("meta-llama/llama-3-70b-instruct");
                    break;
                case "Local API (Ollama)":
                    ModelComboBox.Items.Add("llama3");
                    ModelComboBox.Items.Add("mistral");
                    ModelComboBox.Items.Add("codellama");
                    ModelComboBox.Items.Add("phi3");
                    break;
                case "Local API (KoboldCpp)":
                    ModelComboBox.Items.Add("custom-model");
                    break;
                case "Custom API":
                    ModelComboBox.Items.Add("custom-model");
                    break;
            }
            
            if (ModelComboBox.Items.Count > 0)
            {
                ModelComboBox.SelectedIndex = 0;
            }
        }

        private void UpdateLocalControlsVisibility()
        {
            var selectedApi = (ApiTypeComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString();
            bool isOllama = selectedApi != null && selectedApi.Contains("Ollama", StringComparison.OrdinalIgnoreCase);
            bool isLocal = selectedApi != null && selectedApi.Contains("Local API", StringComparison.OrdinalIgnoreCase);

            if (OllamaPullPanel != null)
                OllamaPullPanel.Visibility = isOllama ? Visibility.Visible : Visibility.Collapsed;
            if (LocalBrowsePanel != null)
                LocalBrowsePanel.Visibility = isLocal ? Visibility.Visible : Visibility.Collapsed;
        }

        private async void TestApiButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ConnectionStatusText.Text = "Testing connection...";
                ConnectionStatusText.Foreground = System.Windows.Media.Brushes.Yellow;
                
                var result = await TestApiConnection();
                
                if (result.Success)
                {
                    ConnectionStatusText.Text = $"✅ Connection successful! Model: {result.ModelName}";
                    ConnectionStatusText.Foreground = System.Windows.Media.Brushes.LightGreen;
                }
                else
                {
                    ConnectionStatusText.Text = $"❌ Connection failed: {result.ErrorMessage}";
                    ConnectionStatusText.Foreground = System.Windows.Media.Brushes.LightCoral;
                }
            }
            catch (Exception ex)
            {
                ConnectionStatusText.Text = $"❌ Error: {ex.Message}";
                ConnectionStatusText.Foreground = System.Windows.Media.Brushes.LightCoral;
            }
        }

        private async void OllamaPullButton_Click(object sender, RoutedEventArgs e)
        {
            var modelName = OllamaModelNameTextBox.Text?.Trim();
            if (string.IsNullOrWhiteSpace(modelName))
            {
                ConnectionStatusText.Text = "❌ Enter a model name to pull.";
                ConnectionStatusText.Foreground = System.Windows.Media.Brushes.LightCoral;
                return;
            }

            try
            {
                ConnectionStatusText.Text = "Pulling model via ollama...";
                ConnectionStatusText.Foreground = System.Windows.Media.Brushes.Yellow;

                var psi = new ProcessStartInfo
                {
                    FileName = "ollama",
                    Arguments = $"pull {modelName}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                string? stderr = null;
                await Task.Run(() =>
                {
                    using var proc = Process.Start(psi);
                    if (proc == null) return;
                    stderr = proc.StandardError.ReadToEnd();
                    proc.WaitForExit();
                    if (proc.ExitCode != 0 && string.IsNullOrWhiteSpace(stderr))
                    {
                        stderr = "ollama pull failed";
                    }
                });

                if (!string.IsNullOrWhiteSpace(stderr))
                {
                    ConnectionStatusText.Text = $"❌ {stderr.Trim()}";
                    ConnectionStatusText.Foreground = System.Windows.Media.Brushes.LightCoral;
                    return;
                }

                EnsureModelInCombo(modelName);
                ConnectionStatusText.Text = "✅ Model pulled and ready.";
                ConnectionStatusText.Foreground = System.Windows.Media.Brushes.LightGreen;
            }
            catch (Exception ex)
            {
                ConnectionStatusText.Text = $"❌ Error: {ex.Message}";
                ConnectionStatusText.Foreground = System.Windows.Media.Brushes.LightCoral;
            }
        }

        private void BrowseLocalModelButton_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Title = "Select local model file",
                Filter = "Model files (*.gguf;*.bin)|*.gguf;*.bin|All files (*.*)|*.*"
            };

            if (dlg.ShowDialog() == true)
            {
                var fileName = Path.GetFileName(dlg.FileName);
                EnsureModelInCombo(fileName);
                ModelComboBox.SelectedItem = fileName;
                ConnectionStatusText.Text = $"✅ Selected local model: {fileName}";
                ConnectionStatusText.Foreground = System.Windows.Media.Brushes.LightGreen;
            }
        }

        private void EnsureModelInCombo(string modelName)
        {
            bool exists = false;
            foreach (var item in ModelComboBox.Items)
            {
                if (string.Equals(item?.ToString(), modelName, StringComparison.OrdinalIgnoreCase))
                {
                    exists = true;
                    break;
                }
            }
            if (!exists)
            {
                ModelComboBox.Items.Add(modelName);
            }
        }

        private async Task<ApiTestResult> TestApiConnection()
        {
            var endpoint = EndpointTextBox.Text.Trim();
            var apiKey = ApiKeyPasswordBox.Password;
            var selectedModel = ModelComboBox.SelectedItem?.ToString();
            var apiType = (ApiTypeComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString();

            if (string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(selectedModel))
            {
                return new ApiTestResult { Success = false, ErrorMessage = "Endpoint and model are required" };
            }

            try
            {
                _httpClient.DefaultRequestHeaders.Clear();

                if (!string.IsNullOrEmpty(apiKey))
                {
                    if (!string.IsNullOrEmpty(apiType) && (apiType.Contains("OpenAI") || apiType.Contains("OpenRouter")))
                    {
                        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
                    }
                    else if (!string.IsNullOrEmpty(apiType) && apiType.Contains("Anthropic"))
                    {
                        _httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);
                    }
                    else if (!string.IsNullOrEmpty(apiType) && apiType.Contains("Google"))
                    {
                        endpoint += $"?key={apiKey}";
                    }
                }

                // Create a simple test request
                var testRequest = new
                {
                    model = selectedModel,
                    messages = new[]
                    {
                        new { role = "user", content = "Hello" }
                    },
                    max_tokens = 5
                };

                var json = JsonConvert.SerializeObject(testRequest);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync($"{endpoint}/chat/completions", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return new ApiTestResult { Success = true, ModelName = selectedModel };
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return new ApiTestResult { Success = false, ErrorMessage = $"HTTP {response.StatusCode}: {errorContent}" };
                }
            }
            catch (Exception ex)
            {
                return new ApiTestResult { Success = false, ErrorMessage = ex.Message };
            }
        }

        private void LoadModelButton_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateConfiguration())
            {
                try
                {
                    CollectModelParameters();

                    var modelConfig = new ModelConfiguration
                    {
                        ApiType = (ApiTypeComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? string.Empty,
                        Endpoint = EndpointTextBox.Text.Trim(),
                        ApiKey = ApiKeyPasswordBox.Password ?? string.Empty,
                        Model = ModelComboBox.SelectedItem?.ToString() ?? string.Empty,
                        Parameters = _modelParameters
                    };
                    
                    // Save to settings (implement this)
                    SaveModelConfiguration(modelConfig);
                    
                    this.DialogResult = true;
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading model: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool ValidateConfiguration()
        {
            if (string.IsNullOrWhiteSpace(EndpointTextBox.Text))
            {
                MessageBox.Show("Please enter an API endpoint.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                EndpointTextBox.Focus();
                return false;
            }

            if (ModelComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please select a model.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                ModelComboBox.Focus();
                return false;
            }

            return true;
        }

        private void CollectModelParameters()
        {
            _modelParameters["MaxTokens"] = (int)MaxTokensSlider.Value;
            _modelParameters["ContextLength"] = (int)ContextLengthSlider.Value;
            _modelParameters["Temperature"] = TemperatureSlider.Value;
            _modelParameters["TopP"] = TopPSlider.Value;
            _modelParameters["FrequencyPenalty"] = FrequencyPenaltySlider.Value;
            _modelParameters["PresencePenalty"] = PresencePenaltySlider.Value;
            _modelParameters["TopK"] = (int)TopKSlider.Value;
            _modelParameters["RepetitionPenalty"] = RepetitionPenaltySlider.Value;
            _modelParameters["EnableStreaming"] = EnableStreamingCheckBox.IsChecked == true;
        }

        private void SaveModelConfiguration(ModelConfiguration config)
        {
            // Save to application settings or file
            // This would integrate with the main application's configuration system
            var settingsPath = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "LLMOverlay",
                "model_config.json");
            
            var json = JsonConvert.SerializeObject(config, Formatting.Indented);
            var directory = System.IO.Path.GetDirectoryName(settingsPath);
            if (!string.IsNullOrEmpty(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
            }
            System.IO.File.WriteAllText(settingsPath, json);
        }

        // Parameter slider event handlers
        private void MaxTokensSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            MaxTokensText.Text = ((int)e.NewValue).ToString();
        }

        private void ContextLengthSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ContextLengthText.Text = ((int)e.NewValue).ToString();
        }

        private void TemperatureSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            TemperatureText.Text = e.NewValue.ToString("F1");
        }

        private void TopPSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            TopPText.Text = e.NewValue.ToString("F2");
        }

        private void FrequencyPenaltySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            FrequencyPenaltyText.Text = e.NewValue.ToString("F1");
        }

        private void PresencePenaltySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            PresencePenaltyText.Text = e.NewValue.ToString("F1");
        }

        private void TopKSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var value = (int)e.NewValue;
            TopKText.Text = value == 0 ? "0 (Disabled)" : value.ToString();
        }

        private void RepetitionPenaltySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            RepetitionPenaltyText.Text = e.NewValue.ToString("F1");
        }

        private void UpdateParameterDisplays()
        {
            MaxTokensText.Text = ((int)MaxTokensSlider.Value).ToString();
            ContextLengthText.Text = ((int)ContextLengthSlider.Value).ToString();
            TemperatureText.Text = TemperatureSlider.Value.ToString("F1");
            TopPText.Text = TopPSlider.Value.ToString("F2");
            FrequencyPenaltyText.Text = FrequencyPenaltySlider.Value.ToString("F1");
            PresencePenaltyText.Text = PresencePenaltySlider.Value.ToString("F1");
            
            var topKValue = (int)TopKSlider.Value;
            TopKText.Text = topKValue == 0 ? "0 (Disabled)" : topKValue.ToString();
            RepetitionPenaltyText.Text = RepetitionPenaltySlider.Value.ToString("F1");
        }

        protected override void OnClosed(EventArgs e)
        {
            _httpClient?.Dispose();
            base.OnClosed(e);
        }
    }

    public class ApiTestResult
    {
        public bool Success { get; set; }
        public string ModelName { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
    }

    public class ModelConfiguration
    {
        public string ApiType { get; set; } = string.Empty;
        public string Endpoint { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();
    }
}