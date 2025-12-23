using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI;
using Microsoft.UI.Composition;
using Microsoft.UI.Composition.SystemBackdrops;

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
        private Slider _settingsTemperatureSlider;

        // Windows API for always-on-top and window positioning
        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const int HWND_TOPMOST = -1;
        private const int SWP_NOSIZE = 0x0001;
        private const int SWP_NOMOVE = 0x0002;
        private const uint SWP_NOACTIVATE = 0x0010;

        public MainWindow()
        {
            this.InitializeComponent();
            _messages = new ObservableCollection<ChatMessage>();
            _llmService = new LLMService();
            
            InitializeWindow();
            SetupEventHandlers();
            MessagesContainer.ItemsSource = _messages;
        }

        private void InitializeWindow()
        {
            // Set window to be always on top
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            SetWindowPos(hwnd, new IntPtr(HWND_TOPMOST), 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_NOACTIVATE);

            // Configure Windows 11 specific window properties
            this.Title = "LLM Chat Overlay";
            this.ExtendsContentIntoTitleBar = true;
            
            // Apply Windows 11 backdrop programmatically
            TryApplySystemBackdrop();
            
            // Calculate initial width (33% of screen)
            UpdateWindowPosition();
        }

        private void TryApplySystemBackdrop()
        {
            try
            {
                // Try to apply Mica backdrop (Windows 11 feature)
                if (MicaBackdrop.IsSupported())
                {
                    this.SystemBackdrop = new MicaBackdrop() { Kind = MicaKind.BaseAlt };
                }
                // Fallback to Acrylic if Mica is not available
                else if (DesktopAcrylicBackdrop.IsSupported())
                {
                    this.SystemBackdrop = new DesktopAcrylicBackdrop();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to apply system backdrop: {ex.Message}");
                // Fallback to solid background if backdrop fails
                this.Background = new SolidColorBrush(Color.FromArgb(200, 20, 20, 20));
            }
        }

        private void SetupEventHandlers()
        {
            // Handle window activation/deactivation
            this.Activated += (s, e) => {
                if (!_isMinimized)
                {
                    UpdateWindowPosition();
                }
            };

            // Handle mouse leave detection for auto-minimize
            RootGrid.PointerExited += (s, e) => {
                if (!_isMinimized)
                {
                    // Delay minimize to allow interaction with UI elements
                    Task.Delay(500).ContinueWith(_ => {
                        DispatcherQueue.TryEnqueue(() => {
                            if (!_isMinimized)
                            {
                                MinimizeToButton();
                            }
                        });
                    });
                }
            };

            // Cancel minimize if mouse re-enters
            RootGrid.PointerEntered += (s, e) => {
                // Cancel any pending minimize
            };
        }

        

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Add welcome message
            AddSystemMessage("Welcome to LLM Chat Overlay! Select a model and start chatting.");
            
            // Set default model selection
            ModelSelector.SelectedIndex = 0;
        }

        private void Window_PointerPressed(object sender, PointerEventArgs e)
        {
            // Simple click-outside detection
            if (!_isMinimized)
            {
                Task.Delay(200).ContinueWith(_ => {
                    DispatcherQueue.TryEnqueue(() => {
                        if (!_isMinimized)
                        {
                            MinimizeToButton();
                        }
                    });
                });
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!_isMinimized)
            {
                UpdateWindowPosition();
            }
        }

        #region UI Event Handlers

        private void ModelSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ModelSelector.SelectedItem is ComboBoxItem selectedItem)
            {
                var modelTag = selectedItem.Tag?.ToString();
                _llmService.SetModel(modelTag ?? "gpt-3.5-turbo");
                AddSystemMessage($"Switched to {selectedItem.Content} model");
            }
        }

        private async void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            // Create settings dialog
            var settingsDialog = new ContentDialog()
            {
                Title = "LLM Settings",
                Content = CreateSettingsContent(),
                CloseButtonText = "Cancel",
                PrimaryButtonText = "Save",
                XamlRoot = this.Content.XamlRoot
            };

            var result = await settingsDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                // Save settings from dialog controls
                _llmService.SetApiKey(_settingsApiKeyInput.Text);
                _llmService.SetEndpoint(_settingsEndpointInput.Text);
                _llmService.SetTemperature(_settingsTemperatureSlider.Value);
                
                await _llmService.SaveSettings();
                AddSystemMessage("Settings saved successfully");
            }
        }

        private UIElement CreateSettingsContent()
        {
            var scrollViewer = new ScrollViewer { HorizontalScrollBarVisibility = ScrollBarVisibility.Auto };
            
            var stackPanel = new StackPanel { Spacing = 10, Margin = new Thickness(10) };

            // API Key input
            var apiKeyLabel = new TextBlock { Text = "API Key:", Margin = new Thickness(0, 0, 0, 5), FontWeight = FontWeights.SemiBold };
            var apiKeyInput = new TextBox 
            { 
                PlaceholderText = "Enter your API key",
                Text = _llmService.GetApiKey(),
                Margin = new Thickness(0, 0, 0, 15)
            };

            // Endpoint URL input
            var endpointLabel = new TextBlock { Text = "Endpoint URL:", Margin = new Thickness(0, 0, 0, 5), FontWeight = FontWeights.SemiBold };
            var endpointInput = new TextBox 
            { 
                PlaceholderText = "https://api.openai.com/v1",
                Text = _llmService.GetEndpoint(),
                Margin = new Thickness(0, 0, 0, 15)
            };

            // Model parameters
            var parametersLabel = new TextBlock { Text = "Model Parameters:", Margin = new Thickness(0, 0, 0, 5), FontWeight = FontWeights.SemiBold };
            
            // Temperature slider
            var tempLabel = new TextBlock { Text = "Temperature: 0.7" };
            var tempSlider = new Slider 
            { 
                Minimum = 0, 
                Maximum = 2, 
                Value = 0.7, 
                StepFrequency = 0.1,
                Width = 200,
                Margin = new Thickness(0, 5, 0, 15)
            };
            tempSlider.ValueChanged += (s, e) => tempLabel.Text = $"Temperature: {e.NewValue:F1}";

            // Save references for later use
            _settingsApiKeyInput = apiKeyInput;
            _settingsEndpointInput = endpointInput;
            _settingsTemperatureSlider = tempSlider;

            stackPanel.Children.Add(apiKeyLabel);
            stackPanel.Children.Add(apiKeyInput);
            stackPanel.Children.Add(endpointLabel);
            stackPanel.Children.Add(endpointInput);
            stackPanel.Children.Add(parametersLabel);
            stackPanel.Children.Add(tempLabel);
            stackPanel.Children.Add(tempSlider);

            scrollViewer.Content = stackPanel;
            return scrollViewer;
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            MinimizeToButton();
        }

        private void FloatingButton_Click(object sender, RoutedEventArgs e)
        {
            ExpandFromButton();
        }

        private void FloatingButton_PointerEntered(object sender, PointerEventArgs e)
        {
            FloatingButton.Background = new SolidColorBrush(Color.FromArgb(230, 40, 40, 40));
        }

        private void FloatingButton_PointerExited(object sender, PointerEventArgs e)
        {
            FloatingButton.Background = new SolidColorBrush(Color.FromArgb(230, 0, 0, 0));
        }

        private async void UploadButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var filePicker = new FileOpenPicker();
                filePicker.ViewMode = PickerViewMode.Thumbnail;
                filePicker.SuggestedStartLocation = PickerLocationId.DocumentsRoot;
                filePicker.FileTypeFilter.Add(".txt");
                filePicker.FileTypeFilter.Add(".jpg");
                filePicker.FileTypeFilter.Add(".png");
                filePicker.FileTypeFilter.Add(".pdf");

                // Get the window handle for the picker
                var windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(this);
                WinRT.Interop.InitializeWithWindow.Initialize(filePicker, windowHandle);

                var file = await filePicker.PickSingleFileAsync();
                if (file != null)
                {
                    await AttachFile(file);
                }
            }
            catch (Exception ex)
            {
                AddSystemMessage($"Error opening file picker: {ex.Message}");
            }
        }

        private async Task AttachFile(StorageFile file)
        {
            try
            {
                var attachment = new MediaAttachment
                {
                    FileName = file.Name,
                    FilePath = file.Path,
                    FileSize = await GetFileSize(file),
                    FileType = file.FileType
                };

                // Generate thumbnail for images
                if (file.FileType.ToLower() == ".jpg" || file.FileType.ToLower() == ".png")
                {
                    attachment.Thumbnail = await GenerateThumbnail(file);
                }

                // Add to current message or create new one
                var lastMessage = _messages.LastOrDefault();
                if (lastMessage?.Sender == "You" && lastMessage.Content == "")
                {
                    // Add to existing empty message
                    lastMessage.Attachments.Add(attachment);
                    lastMessage.HasAttachments = true;
                }
                else
                {
                    // Create new message with attachment
                    var message = new ChatMessage
                    {
                        Sender = "You",
                        Content = "",
                        Attachments = new List<MediaAttachment> { attachment },
                        HasAttachments = true,
                        Alignment = HorizontalAlignment.Right
                    };
                    _messages.Add(message);
                }

                MessagesScrollViewer.ScrollToBottom();
                AddSystemMessage($"Attached: {file.Name}");
            }
            catch (Exception ex)
            {
                AddSystemMessage($"Error attaching file: {ex.Message}");
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            _messages.Clear();
            AddSystemMessage("Chat cleared. Start a new conversation!");
        }

        private void MessageInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            var length = MessageInput.Text.Length;
            CharacterCount.Text = $"{length}/4000";
        }

        private void MessageInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter && !Window.Current.CoreWindow.GetKeyState(Windows.System.VirtualKey.Shift).HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down))
            {
                e.Handled = true;
                SendButton_Click(sender, null);
            }
        }

        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            var message = MessageInput.Text.Trim();
            if (string.IsNullOrEmpty(message) && !HasPendingAttachments())
                return;

            // Add user message
            var userMessage = new ChatMessage
            {
                Sender = "You",
                Content = message,
                Attachments = GetPendingAttachments(),
                HasAttachments = HasPendingAttachments(),
                Alignment = HorizontalAlignment.Right
            };
            _messages.Add(userMessage);

            // Clear input
            MessageInput.Text = "";
            CharacterCount.Text = "0/4000";

            // Scroll to bottom
            MessagesScrollViewer.ScrollToBottom();

            // Show loading
            LoadingOverlay.Visibility = Visibility.Visible;

            try
            {
                // Get LLM response
                var response = await _llmService.SendMessageAsync(message, userMessage.Attachments);
                
                // Add assistant response
                var assistantMessage = new ChatMessage
                {
                    Sender = "Assistant",
                    Content = response,
                    Alignment = HorizontalAlignment.Left
                };
                _messages.Add(assistantMessage);
            }
            catch (Exception ex)
            {
                var errorMessage = new ChatMessage
                {
                    Sender = "System",
                    Content = $"Error: {ex.Message}",
                    SenderColor = new SolidColorBrush(Colors.Red),
                    Alignment = HorizontalAlignment.Center
                };
                _messages.Add(errorMessage);
            }
            finally
            {
                LoadingOverlay.Visibility = Visibility.Collapsed;
                MessagesScrollViewer.ScrollToBottom();
            }
        }

        #endregion

        #region Helper Methods

        private void MinimizeToButton()
        {
            _isMinimized = true;
            MainContentGrid.Visibility = Visibility.Collapsed;
            FloatingButton.Visibility = Visibility.Visible;
            
            // Animate the transition
            var animation = new Windows.UI.Xaml.Media.Animation.Storyboard();
            var fadeOut = new Windows.UI.Xaml.Media.Animation.FadeOutThemeAnimation();
            Windows.UI.Xaml.Media.Animation.Storyboard.SetTarget(fadeOut, MainContentGrid);
            animation.Children.Add(fadeOut);
            
            var fadeIn = new Windows.UI.Xaml.Media.Animation.FadeInThemeAnimation();
            Windows.UI.Xaml.Media.Animation.Storyboard.SetTarget(fadeIn, FloatingButton);
            animation.Children.Add(fadeIn);
            
            animation.Begin();
        }

        private void ExpandFromButton()
        {
            _isMinimized = false;
            FloatingButton.Visibility = Visibility.Collapsed;
            MainContentGrid.Visibility = Visibility.Visible;
            
            UpdateWindowPosition();
            
            // Animate the transition
            var animation = new Windows.UI.Xaml.Media.Animation.Storyboard();
            var fadeOut = new Windows.UI.Xaml.Media.Animation.FadeOutThemeAnimation();
            Windows.UI.Xaml.Media.Animation.Storyboard.SetTarget(fadeOut, FloatingButton);
            animation.Children.Add(fadeOut);
            
            var fadeIn = new Windows.UI.Xaml.Media.Animation.FadeInThemeAnimation();
            Windows.UI.Xaml.Media.Animation.Storyboard.SetTarget(fadeIn, MainContentGrid);
            animation.Children.Add(fadeIn);
            
            animation.Begin();
            
            // Focus on input
            MessageInput.Focus(FocusState.Programmatic);
        }

        private void AddSystemMessage(string content)
        {
            var message = new ChatMessage
            {
                Sender = "System",
                Content = content,
                SenderColor = new SolidColorBrush(Colors.LightGray),
                Alignment = HorizontalAlignment.Center
            };
            _messages.Add(message);
            MessagesScrollViewer.ScrollToBottom();
        }

        private bool HasPendingAttachments()
        {
            // Check if there are attachments waiting to be sent
            return false; // Simplified for this example
        }

        private List<MediaAttachment> GetPendingAttachments()
        {
            // Get pending attachments
            return new List<MediaAttachment>(); // Simplified for this example
        }

        private async Task<string> GetFileSize(StorageFile file)
        {
            var properties = await file.GetBasicPropertiesAsync();
            var size = properties.Size;
            
            if (size < 1024) return $"{size} B";
            if (size < 1024 * 1024) return $"{size / 1024} KB";
            return $"{size / (1024 * 1024)} MB";
        }

        private async Task<string> GenerateThumbnail(StorageFile imageFile)
        {
            // Generate thumbnail for image files
            // This is a simplified implementation
            try
            {
                var thumbnail = await imageFile.GetThumbnailAsync(StorageItemThumbnailMode.SingleItem, 60);
                return thumbnail.Path;
            }
            catch
            {
                return string.Empty;
            }
        }

        #endregion

        private void UpdateWindowPosition()
        {
            var displayArea = this.DisplayArea;
            if (displayArea != null)
            {
                var screenWidth = displayArea.WorkArea.Width;
                var screenHeight = displayArea.WorkArea.Height;
                
                _expandedWidth = screenWidth * 0.33;
                
                if (!_isMinimized)
                {
                    // Ensure window fits within screen bounds
                    this.Width = Math.Min(_expandedWidth, screenWidth - 100);
                    this.Height = Math.Min(screenHeight * 0.8, screenHeight - 100);
                    this.Left = Math.Max(0, screenWidth - this.Width);
                    this.Top = Math.Max(0, (screenHeight - this.Height) / 2);
                }
                else
                {
                    // Position floating button with Windows 11 padding
                    FloatingButton.Margin = new Thickness(0, 0, 20, 0);
                }
            }
        }

        #endregion
    }

    // Data models
    public class ChatMessage
    {
        public string Sender { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Timestamp { get; set; } = DateTime.Now.ToString("HH:mm");
        public SolidColorBrush BackgroundColor { get; set; } = new SolidColorBrush(Color.FromArgb(50, 255, 255, 255));
        public SolidColorBrush SenderColor { get; set; } = new SolidColorBrush(Color.FromArgb(200, 255, 255, 255));
        public HorizontalAlignment Alignment { get; set; } = HorizontalAlignment.Left;
        public List<MediaAttachment> Attachments { get; set; } = new List<MediaAttachment>();
        public bool HasAttachments { get; set; } = false;
    }

    public class MediaAttachment
    {
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string FileSize { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public string Thumbnail { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty; // For text files
    }
}