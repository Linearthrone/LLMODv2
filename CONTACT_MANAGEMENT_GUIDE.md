# Contact Management & Radial Menu Guide

## Overview

The LLM Overlay has been enhanced with a comprehensive contact management system and radial menu, aligning with SillyTavern-style configuration for LLM models. This guide covers all new features and how to use them.

## 🎯 Radial Menu

### Accessing the Radial Menu
- Click the **🎯** (target) button in the main interface tray
- The radial menu appears with 5 action buttons arranged in a circle
- Click the **❌** center button to close the menu

### Radial Menu Buttons

#### 1. Last Contact Buttons (Red & Teal)
- **Red button (top)**: Most recently used contact
- **Teal button (top-right)**: Second most recent contact
- Clicking these immediately loads the contact and starts a conversation
- Tooltips show the contact names

#### 2. Contact Directory (Purple - Right)
- **📇** icon opens the contact directory (rolodex-style interface)
- Browse, search, edit, and delete contacts
- Import/export contact lists
- Double-click any contact to load it immediately

#### 3. Load Model (Orange - Bottom-Right)
- **🤖** icon opens the model configuration window
- Configure API connections and model parameters
- Supports multiple API providers (OpenAI, Anthropic, Google, Mistral, etc.)
- Test API connections before loading

#### 4. Create Contact (Gold - Bottom)
- **➕** icon opens the contact creation wizard
- Create new AI contacts with custom personalities
- Upload avatars and configure system prompts
- Set model-specific parameters

## 👥 Contact Management System

### Contact Data Structure
Each contact contains:
- **Name**: Display name for the contact
- **Base Model**: LLM model to use (GPT-4, Claude, etc.)
- **Physical Description**: Visual appearance description
- **Personality**: Personality traits and behavior
- **Skills**: Special abilities or knowledge areas
- **System Prompt**: Custom system prompt for the AI
- **Avatar**: Profile image or emoji
- **Model Parameters**: Customized generation settings

### Creating Contacts

#### Step 1: Basic Information
1. Click **Create Contact** in the radial menu
2. Enter a **Name** for the contact
3. Select a **Base Model** from the dropdown:
   - OpenAI models (GPT-4, GPT-3.5)
   - Anthropic models (Claude 3 Opus, Sonnet, Haiku)
   - Google models (Gemini Pro)
   - Mistral models
   - Local models (Ollama, KoboldCpp)
   - Custom models

#### Step 2: Avatar Selection
- Click **Browse Avatar** to upload an image
- Supported formats: JPG, PNG, BMP, GIF
- Use **Remove** to clear and reset to default emoji

#### Step 3: Contact Details
- **Physical Description**: How the AI appears or looks
- **Personality**: Character traits, behavior patterns
- **Skills**: Areas of expertise or special knowledge
- **System Prompt**: Custom instructions for the AI

#### Step 4: Save Contact
- Click **Save Contact** to store the contact
- Contacts are saved to `%AppData%\LLMOverlay\contacts\`

### Contact Directory (Rolodex)

#### Features
- **Search**: Filter contacts by name, model, or personality
- **Sort**: Automatically sorted by last active time
- **Actions**: Edit (✏️) or Delete (🗑️) any contact
- **Quick Load**: Double-click to load a contact

#### Import/Export
- **Import**: Load contacts from JSON file
- **Export**: Save all contacts to JSON file
- Format: Standard JSON with all contact properties

## 🤖 Model Configuration (SillyTavern-style)

### API Configuration Tab

#### Supported Providers
1. **OpenAI (ChatGPT)**
   - Endpoint: `https://api.openai.com/v1`
   - Models: GPT-4, GPT-3.5, GPT-3.5-16k

2. **Anthropic (Claude)**
   - Endpoint: `https://api.anthropic.com/v1`
   - Models: Claude 3 Opus, Sonnet, Haiku, Claude 2

3. **Google AI (Gemini)**
   - Endpoint: `https://generativelanguage.googleapis.com/v1`
   - Models: Gemini Pro, Gemini Pro Vision

4. **Mistral AI**
   - Endpoint: `https://api.mistral.ai/v1`
   - Models: Mistral Large, Medium, Small

5. **OpenRouter**
   - Endpoint: `https://openrouter.ai/api/v1`
   - Access to multiple models through unified API

6. **Local APIs**
   - **Ollama**: `http://localhost:11434/v1`
   - **KoboldCpp**: `http://localhost:5001/v1`

#### API Settings
- **Endpoint**: API base URL
- **API Key**: Authentication key (if required)
- **Model**: Specific model to use
- **Test**: Validate connection before loading

### Model Parameters Tab

#### Context Settings
- **Response Length**: Maximum tokens in response (1-8192)
- **Context Length**: Maximum context window (512-32768)

#### Sampler Parameters
- **Temperature**: Creativity/randomness (0.0-2.0)
  - Lower = more predictable
  - Higher = more creative
- **Top P**: Nucleus sampling (0.0-1.0)
- **Frequency Penalty**: Reduce repetition (-2.0 to 2.0)
- **Presence Penalty**: Encourage new topics (-2.0 to 2.0)

### Advanced Settings Tab

#### Streaming Options
- **Enable Streaming**: Show responses as they generate
- Improves user experience for long responses

#### Advanced Sampling
- **Top K**: Limit token choices (0-100, 0 = disabled)
- **Repetition Penalty**: Advanced repetition control (0.0-2.0)

## 📁 Data Storage

### File Locations
```
%AppData%\LLMOverlay\
├── contacts\                    # Contact definitions
│   ├── {contact-id}.json       # Individual contact files
│   └── ...
├── model_config.json           # Current model configuration
└── settings.json              # Application settings
```

### Contact File Format
```json
{
  "Id": "unique-guid",
  "Name": "Assistant Name",
  "BaseModel": "gpt-4",
  "PhysicalDescription": "A helpful AI assistant...",
  "Personality": "Friendly, knowledgeable, patient...",
  "Skills": "Programming, writing, analysis...",
  "SystemPrompt": "You are a helpful AI assistant...",
  "AvatarPath": "path/to/avatar.png",
  "CreatedAt": "2024-01-01T00:00:00",
  "LastActive": "2024-01-01T12:00:00",
  "ModelParameters": {
    "Temperature": 0.7,
    "MaxTokens": 2048,
    "TopP": 0.9,
    "FrequencyPenalty": 0,
    "PresencePenalty": 0
  },
  "ApiEndpoint": "https://api.openai.com/v1",
  "ApiKey": "encrypted-api-key"
}
```

## 🔧 Integration Points

### Main Window Integration
- Radial menu button added to tray
- LoadContact() method for loading contacts
- Contact switching updates the chat interface

### Model Configuration Integration
- Model settings saved to JSON configuration
- API testing before model loading
- Parameter synchronization with contacts

### Storage Integration
- JSON-based storage for portability
- AppData directory for Windows standards compliance
- Automatic backup and recovery considerations

## 🚀 Usage Workflow

### Typical User Session
1. **Open radial menu** (🎯 button)
2. **Select recent contact** or **browse directory**
3. **Load contact** and start conversation
4. **Switch contacts** as needed via radial menu
5. **Adjust model** parameters if required

### Creating New Contacts
1. **Open radial menu** → **Create Contact** (➕)
2. **Configure** all contact properties
3. **Test** with the new contact
4. **Save** for future use

### Model Management
1. **Open radial menu** → **Load Model** (🤖)
2. **Configure API** connection
3. **Set parameters** for desired behavior
4. **Test connection** before applying
5. **Save configuration** for contacts

## 🎨 UI/UX Features

### Glassmorphism Design
- Consistent with existing application theme
- Translucent backgrounds with blur effects
- Smooth animations and transitions

### Accessibility
- High contrast text on glass backgrounds
- Keyboard navigation support
- Clear visual feedback

### Performance
- Lazy loading of contact lists
- Efficient avatar caching
- Optimized API connection testing

## 🔒 Security Considerations

### API Keys
- Stored in local configuration (encrypted in production)
- Not exposed in contact export files
- Secure transmission to API endpoints

### Data Privacy
- All data stored locally
- No cloud synchronization of contacts
- Optional data export for backup

## 🐛 Troubleshooting

### Common Issues

#### Contact Loading Problems
- Check contact file permissions in AppData
- Verify JSON format integrity
- Ensure avatar files exist if specified

#### API Connection Issues
- Validate endpoint URLs
- Check API key validity
- Test network connectivity
- Verify model availability

#### Avatar Display Problems
- Supported formats: JPG, PNG, BMP, GIF
- Maximum file size: 5MB
- File path accessibility

### Debug Information
- Check application logs in AppData
- Verify model configuration JSON
- Test with default contacts/models

## 📝 Future Enhancements

### Planned Features
- [ ] Contact groups and folders
- [ ] Advanced search with filters
- [ ] Contact templates and presets
- [ ] Voice profile integration
- [ ] Multi-language support
- [ ] Cloud synchronization option

### API Provider Expansion
- [ ] Additional model providers
- [ ] Custom API endpoint support
- [ ] Model-specific parameter tuning
- [ ] Batch model testing

### UI Improvements
- [ ] Contact drag-and-drop organization
- [ ] Avatar editing tools
- [ ] Model performance metrics
- [ ] Usage analytics dashboard

---

This guide provides comprehensive documentation for the contact management and radial menu features. For additional support or feature requests, please refer to the project repository or contact the development team.