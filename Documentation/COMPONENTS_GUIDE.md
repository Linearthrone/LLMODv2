# LLM Overlay Components Guide

## Overview
The LLM Overlay now includes advanced components adapted from the LLMAXX project, maintaining the glassmorphism styling while adding powerful functionality.

## Component Tray
Located at the bottom-left of the main interface, the component tray provides quick access to:

- **👥 Character Manager** - Create and manage AI characters
- **🌍 World Info** - Manage lore, character notes, and world settings
- **📊 System Monitor** - Real-time system and service monitoring
- **⚙️ Settings** - Application configuration

## Character Manager Component

### Features
- **Character Creation**: Create new AI characters with custom personalities
- **Character Editing**: Modify existing character attributes
- **Import/Export**: Import characters from JSON files
- **Character Selection**: Select active character for conversations

### Character Properties
- **Name**: Character display name
- **Description**: Detailed character background
- **Personality**: AI behavior traits
- **First Message**: Initial greeting when character is selected
- **Avatar**: Profile image URL (optional)
- **Custom Fields**: Additional character-specific data

### Usage
1. Click the 👥 button in the component tray
2. Create new characters with the "➕ New" button
3. Import existing characters with "📥 Import"
4. Edit characters with the ✏️ button
5. Delete characters with the 🗑️ button
6. Select a character to activate it for chat

### Storage
Characters are stored in:
```
%AppData%\LLMOverlay\characters\[character-id].json
```

## World Info Component

### Features
- **Lore Items**: Manage world lore and information
- **Character Notes**: Keep notes about specific characters
- **World Settings**: Configure world details and context

### Tabs

#### 📚 Lore Items
- Create and manage world lore entries
- Tag-based organization
- Edit and delete lore items
- Search and filtering capabilities

#### 📝 Character Notes
- Keep notes about character interactions
- Organize notes by character
- Timestamp tracking
- Persistent storage

#### ⚙️ World Settings
- **World Name**: Primary world identifier
- **World Description**: Detailed world background
- **Time Period**: Historical setting
- **Location**: Geographic context
- **Custom Settings**: Additional world-specific parameters

### Usage
1. Click the 🌍 button in the component tray
2. Use tabs to navigate between different information types
3. Add new lore items with "➕ Add Lore Item"
4. Create character notes for active conversations
5. Configure world settings for context

### Storage
World data is stored in:
```
%AppData%\LLMOverlay\worldinfo\
├── lore.json (lore items)
├── notes.json (character notes)
└── settings.json (world configuration)
```

## System Monitor Component

### Features
- **Real-time CPU Usage**: Monitor processor utilization
- **Memory Usage**: Track application and system memory
- **Service Status**: Check LLM service connectivity
- **System Information**: Display OS and runtime details
- **Performance Metrics**: Response time monitoring

### Status Indicators
- **🟢 Green**: Service online/healthy
- **🔴 Red**: Service offline/error
- **Response Time**: Millisecond response latency

### Services Monitored
- **OpenAI API**: GPT model service availability
- **Local Model**: Local LLM service status
- **API Response Time**: Request latency metrics

### System Information
- **Operating System**: Windows version and build
- **.NET Runtime**: Framework version
- **Uptime**: Application running time
- **Resource Usage**: CPU and memory consumption

### Usage
1. Click the 📊 button in the component tray
2. Monitor real-time system metrics
3. Check service connectivity status
4. Use 🔄 Refresh to update information
5. Close when done monitoring

### Auto-Update
The system monitor automatically refreshes every 2 seconds while open.

## Integration with LLM Service

All components integrate with the main LLM service:

- **Character Manager**: Sets active character personality for chat
- **World Info**: Provides context for AI responses
- **System Monitor**: Monitors service health and performance

## Styling and Theming

All components maintain the glassmorphism aesthetic:
- **Transparent backgrounds** with gradient overlays
- **Glass effect borders** with subtle highlights
- **Smooth animations** and transitions
- **Dark theme** optimized for overlay usage
- **Consistent styling** with main application

## Technical Implementation

### Architecture
- **WPF UserControls** for modular component design
- **Observable Collections** for dynamic data binding
- **JSON Serialization** for persistent storage
- **Event-driven communication** between components
- **Window management** for modal dialogs

### Data Models
- **CharacterData**: Character information and settings
- **LoreItem**: World lore entries with tags
- **CharacterNote**: Character-specific notes
- **WorldSettings**: Global configuration parameters

### Storage Strategy
- **Local filesystem** using AppData directory
- **JSON format** for human-readable data
- **Atomic operations** to prevent data corruption
- **Backup-friendly** file structure

## Future Enhancements

Planned improvements include:
- **Cloud sync** for character and world data
- **Advanced character AI** with memory
- **Rich media support** in lore items
- **Collaborative features** for shared worlds
- **Performance analytics** and optimization
- **Plugin system** for custom components

## Troubleshooting

### Common Issues
- **Characters not loading**: Check permissions in AppData folder
- **Services showing offline**: Verify API keys and network connectivity
- **High memory usage**: Restart application to clear cache
- **Component windows not opening**: Check .NET runtime version

### Debug Information
Enable debug logging by setting:
```json
{
  "DebugMode": true,
  "LogLevel": "Verbose"
}
```

In settings.json for detailed troubleshooting information.