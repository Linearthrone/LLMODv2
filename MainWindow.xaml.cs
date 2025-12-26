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
using LLMOverlay.Models;
using LLMOverlay.Components;

namespace LLMOverlay
{
    public sealed partial class MainWindow : Window
    {
        private bool _isMinimized = false;
        private double _expandedWidth;
        private double _originalMinWidth;
        private double _originalMinHeight;
        private const double TrayWidth = 50d;
        private string _activeCharacterName = string.Empty;
        private readonly LLMService _llmService;
        private readonly ObservableCollection<ChatMessage> _messages;
        private readonly ObservableCollection<string> _recentAssistantSnippets = new();
        private RadialMenuWindow? _radialMenuWindow;

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

        private Window? _characterWindow;
        private Window? _worldInfoWindow;
        private Window? _systemMonitorWindow;

        public MainWindow()
        {
            InitializeComponent();
            _llmService = new LLMService();
            _messages = new ObservableCollection<ChatMessage>();

            // Assign the UI elements after InitializeComponent
            _settingsApiKeyInput = SettingsApiKeyInput;
            _settingsEndpointInput = SettingsEndpointInput;
            _settingsModelComboBox = SettingsModelComboBox;

            _radialMenuWindow = new RadialMenuWindow(this);

            InitializeChatInterface();
            LoadSettings();
            UpdateCurrentModelDisplay();

            // Set window to be click-through when not in focus
            this.Deactivated += MainWindow_Deactivated;
        }

        private void UpdateRecentAssistantSnippets(string message)
        {
            var snippet = message.Length > 80 ? message.Substring(0, 80) + "..." : message;
            _recentAssistantSnippets.Add(snippet);
            while (_recentAssistantSnippets.Count > 2)
            {
                _recentAssistantSnippets.RemoveAt(0);
            }
        }

        private void UpdateCurrentModelDisplay()
        {
            _radialMenuWindow?.UpdateCurrentModel();
        }

        private void InitializeChatInterface()
        {
            MessagesPanel.Children.Clear();

            var workArea = SystemParameters.WorkArea;
            _expandedWidth = Math.Max(workArea.Width / 3, this.MinWidth);
            _originalMinWidth = this.MinWidth;
            _originalMinHeight = this.MinHeight;

            _isMinimized = true;
            MainContentGrid.Visibility = Visibility.Collapsed;
            MinimizedTray.Visibility = Visibility.Visible;

            this.WindowStartupLocation = WindowStartupLocation.Manual;
            this.MinWidth = TrayWidth;
            this.MinHeight = 0;
            this.Width = TrayWidth;
            this.Height = workArea.Height;
            this.Left = workArea.Right - TrayWidth;
            this.Top = workArea.Top;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // App starts minimized to tray, no need for welcome message yet
            // Welcome message will be shown when user expands the tray
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

        private void MainWindow_Deactivated(object? sender, EventArgs e)
        {
            // Minimize to tray when window loses focus
            MinimizeToTray();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            MinimizeToTray();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MinimizeToTray()
        {
            if (!_isMinimized)
            {
                _isMinimized = true;
                MainContentGrid.Visibility = Visibility.Collapsed;
                MinimizedTray.Visibility = Visibility.Visible;

                var workArea = SystemParameters.WorkArea;
                this.MinWidth = TrayWidth;
                this.MinHeight = 0;
                this.Width = TrayWidth;  // Tray width
                this.Height = workArea.Height;
                this.Left = workArea.Right - TrayWidth;
                this.Top = workArea.Top;
            }
        }

        private void ExpandFromTray(bool showSettingsPanel)
        {
            _isMinimized = false;
            MinimizedTray.Visibility = Visibility.Collapsed;
            MainContentGrid.Visibility = Visibility.Visible;

            var workArea = SystemParameters.WorkArea;
            this.MinWidth = _originalMinWidth;
            this.MinHeight = _originalMinHeight;
            this.Width = Math.Max(_expandedWidth, this.MinWidth);
            this.Height = workArea.Height;
            this.Left = workArea.Right - this.Width;
            this.Top = workArea.Top;

            this.Activate();
            this.Focus();

            if (MessagesPanel.Children.Count == 0)
            {
                AddSystemMessage("Welcome to LLM Overlay! Click minimize or click outside to collapse to tray.");
            }

            if (showSettingsPanel)
            {
                SettingsPanel.Visibility = Visibility.Visible;
            }
        }

        private void MinimizedTray_Click(object sender, MouseButtonEventArgs e)
        {
            if (_isMinimized)
            {
                ExpandFromTray(false);
            }
        }

        private void SettingsToggleButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isMinimized)
            {
                ExpandFromTray(true);
                return;
            }

            if (SettingsPanel.Visibility == Visibility.Collapsed)
            {
                SettingsPanel.Visibility = Visibility.Visible;
            }
            else
            {
                SettingsPanel.Visibility = Visibility.Collapsed;
            }
        }

        public void OpenCharacterManager()
        {
            if (ToggleComponentWindow(ref _characterWindow))
                return;

            var manager = new CharacterManager();
            manager.CharacterSelected += OnCharacterSelected;
            _characterWindow = CreateComponentWindow(manager, 520, 640, () => _characterWindow = null);
        }

        private void CharacterManagerButton_Click(object sender, RoutedEventArgs e)
        {
            OpenCharacterManager();
        }

        private void OnCharacterSelected(CharacterData character)
        {
            _activeCharacterName = character.Name;
            AddSystemMessage($"Active character set to: {character.Name}");
        }

        public void OpenWorldInfo()
        {
            if (ToggleComponentWindow(ref _worldInfoWindow))
                return;

            var worldInfo = new WorldInfo();
            _worldInfoWindow = CreateComponentWindow(worldInfo, 520, 640, () => _worldInfoWindow = null);
        }

        private void WorldInfoButton_Click(object sender, RoutedEventArgs e)
        {
            OpenWorldInfo();
        }

        public void OpenSystemMonitor()
        {
            if (ToggleComponentWindow(ref _systemMonitorWindow))
                return;

            var monitor = new SystemMonitor();
            _systemMonitorWindow = CreateComponentWindow(monitor, 420, 500, () => _systemMonitorWindow = null);
        }

        private void SystemMonitorButton_Click(object sender, RoutedEventArgs e)
        {
            OpenSystemMonitor();
        }

        private Window CreateComponentWindow(UserControl control, double width, double height, Action onClosed)
        {
            var window = new Window
            {
                ShowInTaskbar = true,
                WindowStyle = WindowStyle.None,
                AllowsTransparency = true,
                Background = Brushes.Transparent,
                Width = width,
                Height = height,
                Topmost = true
            };

            var border = new Border
            {
                CornerRadius = new CornerRadius(12),
                BorderBrush = new SolidColorBrush(Color.FromArgb(0x33, 0xFF, 0xFF, 0xFF)),
                BorderThickness = new Thickness(1)
            };

            border.Background = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 1),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop((Color)ColorConverter.ConvertFromString("#E6000000"), 0),
                    new GradientStop((Color)ColorConverter.ConvertFromString("#E6111111"), 1),
                    new GradientStop((Color)ColorConverter.ConvertFromString("#22FFFFFF"), 0.5)
                }
            };

            border.Child = control;
            window.Content = border;

            var workArea = SystemParameters.WorkArea;
            var targetTop = workArea.Bottom - height - 80;
            window.Left = workArea.Left + 12;
            window.Top = targetTop < workArea.Top + 12 ? workArea.Top + 12 : targetTop;

            window.Closed += (_, __) => onClosed();
            window.Show();
            return window;
        }

        private bool ToggleComponentWindow(ref Window? windowField)
        {
            if (windowField != null)
            {
                windowField.Close();
                windowField = null;
                return true;
            }

            return false;
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
            {
                CharacterCounter.Foreground = new SolidColorBrush(Color.FromRgb(255, 100, 100));
            }
            else if (textLength > 3000)
            {
                CharacterCounter.Foreground = new SolidColorBrush(Color.FromRgb(255, 200, 100));
            }
            else
            {
                CharacterCounter.Foreground = new SolidColorBrush(Color.FromRgb(136, 255, 136));
            }
        }

        private void MessageInputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !Keyboard.IsKeyDown(Key.LeftShift) && !Keyboard.IsKeyDown(Key.RightShift))
            {
                e.Handled = true;
                _ = SendMessage();
            }
        }

        private string GetSelectedModel()
        {
            if (SettingsModelComboBox.SelectedItem is ComboBoxItem item && item.Content != null)
            {
                return item.Content.ToString() ?? "gpt-3.5-turbo";
            }
            return "gpt-3.5-turbo";
        }

        private void SelectModelInCombo(string modelName)
        {
            foreach (var item in SettingsModelComboBox.Items)
            {
                if (item is ComboBoxItem comboItem && comboItem.Content != null &&
                    string.Equals(comboItem.Content.ToString(), modelName, StringComparison.OrdinalIgnoreCase))
                {
                    SettingsModelComboBox.SelectedItem = comboItem;
                    return;
                }
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

            UpdateRecentAssistantSnippets(message);
            UpdateCurrentModelDisplay();
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

        private string GetSettingsFilePath()
        {
            return System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LLMOverlay", "settings.json");
        }

        private void SaveSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            SaveSettings();
        }

        private void LoadSettings()
        {
            try
            {
                var settingsPath = GetSettingsFilePath();

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
                        {
                            SelectModelInCombo(settings["Model"]);
                        }

                        _llmService.UpdateSettings(settings);
                    }
                }

                if (string.IsNullOrWhiteSpace(SettingsApiKeyInput.Text))
                {
                    SettingsApiKeyInput.Text = _llmService.GetApiKey();
                }

                if (string.IsNullOrWhiteSpace(SettingsEndpointInput.Text))
                {
                    SettingsEndpointInput.Text = _llmService.GetEndpoint();
                }

                SelectModelInCombo(_llmService.GetCurrentModel());
                UpdateCurrentModelDisplay();
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
                var modelName = GetSelectedModel();
                var settings = new Dictionary<string, string>
                {
                    ["ApiKey"] = SettingsApiKeyInput.Text ?? "",
                    ["Endpoint"] = SettingsEndpointInput.Text ?? "",
                    ["Model"] = modelName
                };

                var settingsPath = GetSettingsFilePath();
                var settingsDirectory = System.IO.Path.GetDirectoryName(settingsPath);
                if (!string.IsNullOrEmpty(settingsDirectory))
                {
                    System.IO.Directory.CreateDirectory(settingsDirectory);
                }

                var settingsFile = settingsPath;
                var settingsJson = JsonConvert.SerializeObject(settings, Formatting.Indented);

                System.IO.File.WriteAllText(settingsFile, settingsJson);

                // Update LLM service with new settings
                _llmService.UpdateSettings(settings);
                UpdateCurrentModelDisplay();

                AddSystemMessage("Settings saved successfully!");
                SettingsPanel.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                AddSystemMessage($"Failed to save settings: {ex.Message}");
            }
        }

        public void ShowFromRadial(bool showSettingsPanel)
        {
            ExpandFromTray(showSettingsPanel);
        }

        public void ShowPersonaComingSoon()
        {
            AddSystemMessage("Persona creation: configure prompts and save presets (ollama or HuggingFace). Coming soon.");
        }

        public void LoadContact(Contact contact)
        {
            // Load the contact into the chat interface
            // This would update the chat interface to use the selected contact
            if (contact != null)
            {
                AddSystemMessage($"Loaded contact: {contact.Name} ({contact.BaseModel})");
                // Update the interface to show the contact's avatar and info
                // Set the active model configuration based on the contact's settings
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _radialMenuWindow?.Close();
        }

        private void RadialMenuButton_Click(object sender, RoutedEventArgs e)
        {
            ShowRadialMenu(sender as FrameworkElement);
        }

        private void ShowRadialMenu(FrameworkElement? anchor = null)
        {
            if (_radialMenuWindow == null || !_radialMenuWindow.IsLoaded)
            {
                _radialMenuWindow = new RadialMenuWindow(this);
                _radialMenuWindow.Closed += (_, __) => _radialMenuWindow = null;
            }

            var workArea = SystemParameters.WorkArea;
            var menuWidth = _radialMenuWindow.ActualWidth > 0 ? _radialMenuWindow.ActualWidth : _radialMenuWindow.Width;
            var menuHeight = _radialMenuWindow.ActualHeight > 0 ? _radialMenuWindow.ActualHeight : _radialMenuWindow.Height;

            double left;
            double top;

            if (anchor != null)
            {
                var anchorCenter = anchor.TranslatePoint(new Point(anchor.ActualWidth / 2, anchor.ActualHeight / 2), this);
                var screenPoint = PointToScreen(anchorCenter);
                var source = PresentationSource.FromVisual(this);
                if (source?.CompositionTarget != null)
                {
                    screenPoint = source.CompositionTarget.TransformFromDevice.Transform(screenPoint);
                }

                left = screenPoint.X - (menuWidth / 2);
                top = screenPoint.Y - (menuHeight / 2);
            }
            else
            {
                left = workArea.Right - menuWidth - TrayWidth - 10;
                top = workArea.Bottom - menuHeight - 60;
            }

            left = Math.Max(workArea.Left, Math.Min(left, workArea.Right - menuWidth));
            top = Math.Max(workArea.Top, Math.Min(top, workArea.Bottom - menuHeight));

            _radialMenuWindow.Left = left;
            _radialMenuWindow.Top = top;

            if (!_radialMenuWindow.IsVisible)
            {
                _radialMenuWindow.Show();
            }
            else
            {
                _radialMenuWindow.Activate();
            }
        }
    }
}
