# Quick Start Guide - WinUI 3 LLM Desktop Overlay

## ⚠️ Important Note

The current XAML files are experiencing compilation issues. This guide provides the fastest path to a working application.

## 🚀 Fastest Path to Success

### Method 1: Use Visual Studio Template (Recommended)

1. **Open Visual Studio 2022**
2. **Create New Project** → Search for "WinUI 3"
3. **Select**: "Blank App, Packaged (WinUI 3 in Desktop)"
4. **Name**: LLMOverlay
5. **Click Create**

6. **Replace generated files** with these from this repository:
   - Copy `LLMService.cs` → Add to your project
   - Copy `App.xaml.cs` logic → Merge with generated file
   - Copy `MainWindow.xaml.cs` logic → Merge with generated file
   - **Gradually add XAML features** from `MainWindow.xaml`

7. **Build and Run** - Should work immediately!

### Method 2: Fix Current Project

If you want to fix the current project:

1. **Replace MainWindow.xaml** with minimal version:

```xml
as

2. **Simplify MainWindow.xaml.cs**:

```csharp
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Runtime.InteropServices;

namespace LLMOverlay
{
    public sealed partial class MainWindow : Window
    {
        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, 
            int X, int Y, int cx, int cy, uint uFlags);

        private const int HWND_TOPMOST = -1;
        private const int SWP_NOSIZE = 0x0001;
        private const int SWP_NOMOVE = 0x0002;
        private const uint SWP_NOACTIVATE = 0x0010;

        private readonly LLMService _llmService;

        public MainWindow()
        {
            this.InitializeComponent();
            _llmService = new LLMService();
            
            // Set always on top
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            SetWindowPos(hwnd, new IntPtr(HWND_TOPMOST), 0, 0, 0, 0, 
                SWP_NOSIZE | SWP_NOMOVE | SWP_NOACTIVATE);
            
            // Position window
            this.Width = 400;
            this.Height = 600;
        }

        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            var message = MessageInput.Text;
            if (string.IsNullOrWhiteSpace(message)) return;

            // Add user message
            AddMessage("You", message);
            MessageInput.Text = "";

            // Get LLM response
            try
            {
                var response = await _llmService.SendMessageAsync(message);
                AddMessage("Assistant", response);
            }
            catch (Exception ex)
            {
                AddMessage("Error", ex.Message);
            }
        }

        private void AddMessage(string sender, string content)
        {
            var messageBlock = new TextBlock
            {
                Text = $"{sender}: {content}",
                TextWrapping = TextWrapping.Wrap,
                Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 255, 255)),
                Margin = new Thickness(0, 0, 0, 10)
            };
            MessagesPanel.Children.Add(messageBlock);
        }
    }
}
```

3. **Build** - Should work now!

## 📋 Required NuGet Packages

```xml
<PackageReference Include="Microsoft.WindowsAppSDK" Version="1.4.231008000" />
<PackageReference Include="Microsoft.Windows.SDK.BuildTools" Version="10.0.22621.2428" />
<PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
```

## 🎯 Core Features to Add Incrementally

Once the basic version works, add features one at a time:

1. ✅ **Basic chat** (working above)
2. ➕ **Minimize to button** (add floating button)
3. ➕ **File attachments** (add file picker)
4. ➕ **Settings dialog** (add ContentDialog)
5. ➕ **Styling** (add glassmorphism effects)
6. ➕ **Animations** (add transitions)
7. ➕ **Model selection** (add ComboBox)

## 🔧 Troubleshooting

### Build Still Fails?
1. Clean solution: `dotnet clean`
2. Delete `bin` and `obj` folders
3. Restart Visual Studio
4. Rebuild

### XAML Errors?
- Remove complex bindings
- Remove custom templates
- Use simple controls first
- Add complexity gradually

### Runtime Errors?
- Check API keys in settings
- Verify .NET 8 runtime installed
- Check Windows 11 version (22000+)

## 📚 Resources

- **WinUI 3 Docs**: https://learn.microsoft.com/en-us/windows/apps/winui/winui3/
- **Windows App SDK**: https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/
- **Sample Apps**: https://github.com/microsoft/WindowsAppSDK-Samples

## 💡 Pro Tips

1. **Start Simple**: Get basic functionality working first
2. **Test Frequently**: Build after each feature addition
3. **Use Debugger**: Set breakpoints to understand flow
4. **Check Logs**: Look at Output window for errors
5. **Community Help**: WinUI 3 Discord and Stack Overflow

## ✅ Success Checklist

- [ ] Visual Studio 2022 installed
- [ ] .NET 8 SDK installed
- [ ] Windows 11 (Build 22000+)
- [ ] Project builds successfully
- [ ] Window appears on screen
- [ ] Always-on-top works
- [ ] Can send messages
- [ ] LLM responds (with API key)

Good luck! 🚀