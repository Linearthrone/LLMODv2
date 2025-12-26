# LLM Desktop Overlay - WPF Chat Interface (Windows)

A modern, feature-rich desktop overlay application for interacting with Large Language Models (LLMs) directly from your Windows 11 desktop. This overlay provides a sleek chat interface with Windows 11 design language, stays on top of other applications, and minimizes to a floating button when not in use.

## Features

### 🎯 Core Functionality
- **Always-On-Top Overlay**: Stays above all other applications for quick access
- **Smart Minimization**: Automatically minimizes to floating button when clicked outside
- **33% Screen Width**: Takes up exactly one-third of your screen when expanded
- **Multi-Model Support**: Compatible with GPT-3.5, GPT-4, Claude-3, and local LLMs
- **Real-Time Chat**: Smooth, responsive chat interface with message history

### 💬 Chat Features
- **Rich Message Display**: Properly formatted chat bubbles with timestamps
- **File Attachments**: Support for text files, images, and documents
- **Media Preview**: Thumbnail previews for images and file information display
- **Conversation History**: Maintains chat context for better responses
- **Character Counter**: Real-time character count with 4000 character limit

### 🎨 Design & UX
- **Glassmorphism UI**: Modern frosted glass effect with transparency
- **Dark Theme**: Easy on the eyes dark interface design
- **Smooth Animations**: Fluid transitions and micro-interactions
- **Responsive Layout**: Adapts to different screen sizes and resolutions
- **Keyboard Shortcuts**: Enter to send, Shift+Enter for new line

### ⚙️ Configuration
- **Model Selection**: Easy dropdown to switch between LLM providers
- **API Key Management**: Secure storage of API keys and endpoints
- **Custom Endpoints**: Support for custom API endpoints and local models
- **Temperature Control**: Adjustable response creativity and randomness
- **Settings Persistence**: Configuration saved between sessions

## Requirements

### System Requirements
- **Windows 10 version 1903** or higher (Windows 10/11 compatible)
- **.NET 8.0 Runtime** or higher
- **DirectX 9** compatible graphics card

### Development Requirements
- **Visual Studio 2022** or **Visual Studio 2019** with:
  - .NET desktop development workload
  - WPF development support
- **Windows 10 or 11** development environment

## Installation

### Option 1: Build from Source
1. **Clone or download** the project files
2. **Open Visual Studio 2022**
3. **Load the `LLMOverlay.csproj` file**
4. **Restore NuGet packages** (Build → Restore NuGet Packages)
5. **Build the solution** (Build → Build Solution or Ctrl+Shift+B)
6. **Run the application** (F5 or Start Debugging)

### Option 2: Run from Compiled Executable
1. **Navigate to the build output folder** (`bin/x64/Release/net8.0-windows10.0.26100.0/`)
2. **Run `LLMOverlay.exe`**
3. **Grant necessary permissions** when prompted
4. **Note**: Requires .NET 8.0 Runtime to be installed on the system

### Compatibility
- **Framework**: .NET 8.0 (LTS - supported until November 2026)
- **Windows App SDK**: 1.7 (latest stable)
- **Target Platform**: Windows 11 Build 26100+ (Windows 11 23H2)

### Windows 11 Features Used
- **Mica Backdrop**: Native Windows 11 translucent background effect
- **Rounded Corners**: Modern window styling
- **Fluent Design Icons**: Updated iconography matching Windows 11
- **Enhanced DPI Awareness**: Optimized for high-resolution displays
- **WinUI 3 Controls**: Latest Windows UI framework

## Setup & Configuration

### 1. Initial Configuration
When you first run the application:
1. Click the **⚙️ Settings button** in the header
2. **Enter your API key** for the chosen LLM provider
3. **Configure the endpoint URL** (if using a custom endpoint)
4. **Adjust temperature settings** (0.0 = conservative, 2.0 = creative)
5. Click **Save** to apply settings

### 2. API Key Setup

#### OpenAI (GPT Models)
- **API Key**: Get from [OpenAI Platform](https://platform.openai.com/api-keys)
- **Endpoint**: `https://api.openai.com/v1` (default)
- **Models**: GPT-3.5 Turbo, GPT-4

#### Anthropic (Claude)
- **API Key**: Get from [Anthropic Console](https://console.anthropic.com/)
- **Endpoint**: `https://api.anthropic.com/v1` (default)
- **Models**: Claude-3

#### Local LLMs
- **API Key**: Not required for local models
- **Endpoint**: `http://localhost:1234/v1` (or your local server)
- **Requirements**: Local LLM server (Ollama, LM Studio, etc.)

## Usage Guide

### Basic Usage
1. **Launch the application** - it will appear as a sidebar on the right
2. **Select your LLM model** from the dropdown in the header
3. **Type your message** in the input area at the bottom
4. **Press Enter** to send or click the ➤ button
5. **View responses** in the chat area above

### File Attachments
1. **Click the 📎 button** to open file picker
2. **Select files** (supports text, images, PDFs)
3. **Type your message** along with the attachment
4. **Send** to have the LLM process the files
5. **View file previews** in chat bubbles

### Overlay Management
- **Expand**: Click the floating 💬 button to expand the overlay
- **Minimize**: Click the − button or click outside the overlay
- **Always On Top**: The overlay stays above other applications
- **Auto-Minimize**: Click outside to automatically minimize after 500ms

### Keyboard Shortcuts
- **Enter**: Send message
- **Shift + Enter**: New line in message
- **Escape**: Minimize to floating button (when implemented)

## File Structure

```
LLMOverlay/
├── App.xaml                 # Application entry point XAML
├── App.xaml.cs             # Application entry point code
├── MainWindow.xaml         # Main overlay interface XAML
├── MainWindow.xaml.cs      # Main overlay functionality
├── LLMService.cs           # LLM communication service
├── LLMOverlay.csproj       # Project configuration
├── app.manifest            # Windows application manifest
├── Styles/
│   └── Styles.xaml         # UI styling and themes
├── Assets/                 # Application assets (icons, images)
├── Properties/
│   └── launchSettings.json # Development settings
└── README.md               # This file
```

## API Integration Details

### OpenAI Integration
```csharp
// Example API call structure
{
  "model": "gpt-3.5-turbo",
  "messages": [
    {"role": "user", "content": "Hello!"},
    {"role": "assistant", "content": "How can I help you?"}
  ],
  "temperature": 0.7,
  "max_tokens": 2000
}
```

### Claude Integration
```csharp
// Example API call structure
{
  "model": "claude-3-sonnet-20240229",
  "max_tokens": 2000,
  "messages": [
    {"role": "user", "content": "Hello!"},
    {"role": "assistant", "content": "How can I help you?"}
  ]
}
```

## Build Notes

The build process may show warnings about package versions being resolved differently than specified. These warnings are normal and indicate that newer versions of the Windows App SDK and Windows SDK Build Tools were found and used instead of the exact versions requested. This is safe and does not affect functionality.

## Troubleshooting

### Common Issues

#### Application Won't Start
- **Check Windows version**: Ensure you're running Windows 11 (Build 22000+)
- **Install .NET 8**: Download from Microsoft's website if not installed
- **Update Windows**: Ensure you have the latest Windows 11 updates
- **Run as administrator**: Right-click → "Run as administrator"

#### API Connection Errors
- **Verify API key**: Ensure the API key is correct and active
- **Check endpoint URL**: Confirm the endpoint is reachable
- **Network connectivity**: Ensure internet connection is stable
- **Rate limits**: Check if you've hit API rate limits

#### Overlay Issues
- **Screen resolution**: Application works best on 1920x1080 or higher
- **Multiple monitors**: The overlay appears on the primary monitor
- **Display scaling**: Optimized for Windows 11 scaling (100%-200%)
- **Mica backdrop not working**: Ensure transparency effects are enabled in Windows 11 settings
- **Rounded corners missing**: Check if Windows 11 visual effects are enabled

#### File Attachment Issues
- **File size**: Large files may take time to process
- **File format**: Ensure file formats are supported
- **Permissions**: Check file access permissions

### Debug Mode
For debugging:
1. **Open in Visual Studio**
2. **Set breakpoints** in `MainWindow.xaml.cs` or `LLMService.cs`
3. **Check Output window** for error messages
4. **Use Debug → Start Debugging** (F5)

## Development & Customization

### Adding New LLM Providers
1. **Update `LLMService.cs`**:
   - Add new model to `SetModel()` method
   - Create new request method (`SendNewProviderRequest()`)
   - Add response model classes

2. **Update `MainWindow.xaml`**:
   - Add new ComboBoxItem to ModelSelector
   - Set appropriate Tag value

### Customizing the UI
1. **Modify `Styles/Styles.xaml`** for visual changes
2. **Edit `MainWindow.xaml`** for layout modifications
3. **Update colors and effects** in the resource dictionary

### Adding Features
- **New file types**: Extend `ProcessAttachments()` method
- **Additional settings**: Modify `CreateSettingsContent()` method
- **Keyboard shortcuts**: Add to `KeyDown` event handlers

## Performance Notes

- **Memory Usage**: ~60-120 MB when idle (slightly higher due to Mica backdrop)
- **CPU Usage**: Minimal during chat, spike during API calls and backdrop rendering
- **GPU Usage**: Low usage for Mica effect and smooth animations
- **Network**: API calls use ~10-100 KB per message
- **Storage**: Settings stored in local Windows settings
- **Windows 11 Integration**: Optimized for Windows 11 compositor and display stack

## Privacy & Security

- **API Keys**: Stored locally in Windows application data
- **Chat History**: Stored only in memory, cleared on exit
- **File Access**: Temporary access only during processing
- **Network Calls**: Direct to configured endpoints, no intermediaries

## License

This project is provided as-is for educational and development purposes. Please respect the terms of service of the LLM providers you use.

## Support

For issues and questions:
1. **Check this README** for troubleshooting steps
2. **Review the code comments** for implementation details
3. **Test with different API providers** to isolate issues
4. **Ensure all requirements are met** before reporting issues

---

**Enjoy your LLM desktop overlay experience!** 🚀