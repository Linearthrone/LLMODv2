using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace LLMOverlay
{
    public sealed partial class MainWindow : Window
    {
        private bool _isMinimized = false;
        private double _expandedWidth;
        private readonly LLMService _llmService;
        private readonly ObservableCollection<ChatMessage> _messages;
        
        // Settings dialog controls
        private TextBox _settingsApiKeyInput;
        private TextBox _settingsEndpointInput;
        private ComboBox _settingsModelComboBox;
        
        // WPF specific Win32 API calls for window transparency and behavior
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern bool SetLayeredWindowAttributes(IntPtr hWnd, uint crKey, byte bAlpha, uint dwFlags);

        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_TRANSPARENT = 0x00000020;
        private const int WS_EX_LAYERED = 0x00080000;
        private const int LWA_COLORKEY = 0x00000001;
        private const int LWA_ALPHA = 0x00000002;

        public MainWindow()
        {
            InitializeComponent();
            _llmService = new LLMService();
            _messages = new ObservableCollection<ChatMessage>();
            
            // Assign the UI elements after InitializeComponent
            _settingsApiKeyInput = SettingsApiKeyInput;
            _settingsEndpointInput = SettingsEndpointInput;
            _settingsModelComboBox = SettingsModelComboBox;
            
            InitializeChatInterface();
            LoadSettings();
            
            // Set window to be click-through when not in focus
            this.Deactivated += MainWindow_Deactivated;
        }

        private void InitializeChatInterface()
        {
            // Initialize message display
            MessagesPanel.Children.Clear();
            
            // Set initial window width to 1/3 of screen
            var screenWidth = SystemParameters.PrimaryScreenWidth;
            _expandedWidth = screenWidth / 3;
            this.Width = _expandedWidth;
            this.Height = SystemParameters.PrimaryScreenHeight * 0.8;
            
            // Position window on the right side
            this.Left = screenWidth - _expandedWidth;
            this.Top = (SystemParameters.PrimaryScreenHeight - this.Height) / 2;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Add welcome message
            AddSystemMessage("Welcome to LLM Overlay! Click outside to minimize.");
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Update layout when window size changes
        }

        private void MainWindow_Deactivated(object sender, EventArgs e)
        {
            // Minimize to floating button when window loses focus
            MinimizeToFloatingButton();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            MinimizeToFloatingButton();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MinimizeToFloatingButton()
        {
            if (!_isMinimized)
            {
                _isMinimized = true;
                MainContentGrid.Visibility = Visibility.Collapsed;
                FloatingButton.Visibility = Visibility.Visible;
                this.Width = 60;
                this.Height = 60;
                
                // Position floating button
                var screenWidth = SystemParameters.PrimaryScreenWidth;
                this.Left = screenWidth - 70;
                this.Top = 10;
            }
        }

        private void FloatingButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isMinimized)
            {
                _isMinimized = false;
                FloatingButton.Visibility = Visibility.Collapsed;
                MainContentGrid.Visibility = Visibility.Visible;
                this.Width = _expandedWidth;
                this.Height = SystemParameters.PrimaryScreenHeight * 0.8;
                
                // Position main window
                var screenWidth = SystemParameters.PrimaryScreenWidth;
                this.Left = screenWidth - _expandedWidth;
                this.Top = (SystemParameters.PrimaryScreenHeight - this.Height) / 2;
                
                this.Activate();
            }
        }

        private void SettingsToggleButton_Click(object sender, RoutedEventArgs e)
        {
            if (SettingsPanel.Visibility == Visibility.Collapsed)
            {
                SettingsPanel.Visibility = Visibility.Visible;
                SettingsToggleButton.Content = "💬";
            }
            else
            {
                SettingsPanel.Visibility = Visibility.Collapsed;
                SettingsToggleButton.Content = "⚙️";
            }
        }

        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            await SendMessage();
        }

        private void MessageInputBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textLength = MessageInputBox.Text.Length;
            CharacterCounter.Text = $"{textLength} / 4000";
            
            // Change color based on character count
            if (textLength > 3500)
                CharacterCounter.Foreground = new SolidColorBrush(Color.FromRgb(255, 100, 100));
            else if (textLength > 3000)
                CharacterCounter.Foreground = new SolidColorBrush(Color.FromRgb(255, 200, 100));
            else
                CharacterCounter.Foreground = new SolidColorBrush(Color.FromRgb(136, 255, 136));
        }

        private void MessageInputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !Keyboard.IsKeyDown(Key.LeftShift) && !Keyboard.IsKeyDown(Key.RightShift))
            {
                e.Handled = true;
                _ = SendMessage();
            }
        }

        private async Task SendMessage()
        {
            var message = MessageInputBox.Text?.Trim();
            if (string.IsNullOrEmpty(message))
                return;

            // Clear input
            MessageInputBox.Text = "";
            CharacterCounter.Text = "0 / 4000";

            // Add user message
            AddUserMessage(message);

            // Show typing indicator
            AddSystemMessage("AI is thinking...");

            try
            {
                // Get response from LLM service
                var response = await _llmService.SendMessageAsync(message);
                
                // Remove typing indicator and add response
                RemoveLastSystemMessage();
                AddAssistantMessage(response);
            }
            catch (Exception ex)
            {
                RemoveLastSystemMessage();
                AddSystemMessage($"Error: {ex.Message}");
            }
        }

        private void AddUserMessage(string message)
        {
            var messageBorder = new Border
            {
                Style = (Style)this.FindResource("UserMessageBubble")
            };

            var textBlock = new TextBlock
            {
                Text = message,
                TextWrapping = TextWrapping.Wrap,
                Foreground = Brushes.White,
                FontSize = 14,
                FontFamily = new FontFamily("Segoe UI")
            };

            messageBorder.Child = textBlock;
            MessagesPanel.Children.Add(messageBorder);
            
            // Scroll to bottom
            ChatScrollViewer.ScrollToBottom();
        }

        private void AddAssistantMessage(string message)
        {
            var messageBorder = new Border
            {
                Style = (Style)this.FindResource("AssistantMessageBubble")
            };

            var textBlock = new TextBlock
            {
                Text = message,
                TextWrapping = TextWrapping.Wrap,
                Foreground = Brushes.White,
                FontSize = 14,
                FontFamily = new FontFamily("Segoe UI")
            };

            messageBorder.Child = textBlock;
            MessagesPanel.Children.Add(messageBorder);
            
            // Scroll to bottom
            ChatScrollViewer.ScrollToBottom();
        }

        private void AddSystemMessage(string message)
        {
            var messageBorder = new Border
            {
                Style = (Style)this.FindResource("SystemMessageBubble")
            };

            var textBlock = new TextBlock
            {
                Text = message,
                TextWrapping = TextWrapping.Wrap,
                Foreground = Brushes.LightGray,
                FontSize = 12,
                FontFamily = new FontFamily("Segoe UI"),
                FontStyle = FontStyles.Italic,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            messageBorder.Child = textBlock;
            MessagesPanel.Children.Add(messageBorder);
            
            // Scroll to bottom
            ChatScrollViewer.ScrollToBottom();
        }

        private void RemoveLastSystemMessage()
        {
            if (MessagesPanel.Children.Count > 0)
            {
                var lastChild = MessagesPanel.Children[MessagesPanel.Children.Count - 1];
                if (lastChild is Border border && border.Style == this.FindResource("SystemMessageBubble"))
                {
                    MessagesPanel.Children.Remove(lastChild);
                }
            }
        }

        private void SaveSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            SaveSettings();
        }

        private void LoadSettings()
        {
            try
            {
                // Load settings from local storage
                var settingsPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LLMOverlay", "settings.json");
                
                if (System.IO.File.Exists(settingsPath))
                {
                    var settingsJson = System.IO.File.ReadAllText(settingsPath);
                    var settings = JsonConvert.DeserializeObject<Dictionary<string, string>>(settingsJson);
                    
                    if (settings != null)
                    {
                        if (settings.ContainsKey("ApiKey"))
                            SettingsApiKeyInput.Text = settings["ApiKey"];
                        
                        if (settings.ContainsKey("Endpoint"))
                            SettingsEndpointInput.Text = settings["Endpoint"];
                        
                        if (settings.ContainsKey("Model"))
                            SettingsModelComboBox.SelectedItem = settings["Model"];
                    }
                }
            }
            catch (Exception ex)
            {
                AddSystemMessage($"Failed to load settings: {ex.Message}");
            }
        }

        private void SaveSettings()
        {
            try
            {
                var settings = new Dictionary<string, string>
                {
                    ["ApiKey"] = SettingsApiKeyInput.Text ?? "",
                    ["Endpoint"] = SettingsEndpointInput.Text ?? "",
                    ["Model"] = SettingsModelComboBox.SelectedItem?.ToString() ?? "gpt-3.5-turbo"
                };

                var settingsPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LLMOverlay");
                System.IO.Directory.CreateDirectory(settingsPath);
                
                var settingsFile = System.IO.Path.Combine(settingsPath, "settings.json");
                var settingsJson = JsonConvert.SerializeObject(settings, Formatting.Indented);
                
                System.IO.File.WriteAllText(settingsFile, settingsJson);
                
                // Update LLM service with new settings
                _llmService.UpdateSettings(settings);
                
                AddSystemMessage("Settings saved successfully!");
                SettingsPanel.Visibility = Visibility.Collapsed;
                SettingsToggleButton.Content = "⚙️";
            }
            catch (Exception ex)
            {
                AddSystemMessage($"Failed to save settings: {ex.Message}");
            }
        }
    }
}