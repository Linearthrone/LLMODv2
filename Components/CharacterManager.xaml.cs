using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Newtonsoft.Json;

namespace LLMOverlay.Components
{
    public partial class CharacterManager : UserControl
    {
        public ObservableCollection<CharacterData> Characters { get; set; }
        public event Action<CharacterData> CharacterSelected;
        
        public CharacterManager()
        {
            InitializeComponent();
            Characters = new ObservableCollection<CharacterData>();
            DataContext = this;
            LoadCharacters();
            RefreshCharacterList();
        }

        private void LoadCharacters()
        {
            try
            {
                var charactersPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LLMOverlay", "characters");
                
                if (Directory.Exists(charactersPath))
                {
                    var characterFiles = Directory.GetFiles(charactersPath, "*.json");
                    
                    foreach (var file in characterFiles)
                    {
                        var json = File.ReadAllText(file);
                        var character = JsonConvert.DeserializeObject<CharacterData>(json);
                        if (character != null)
                        {
                            Characters.Add(character);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading characters: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshCharacterList()
        {
            CharacterListPanel.Children.Clear();
            
            foreach (var character in Characters)
            {
                var characterCard = CreateCharacterCard(character);
                CharacterListPanel.Children.Add(characterCard);
            }
        }

        private Border CreateCharacterCard(CharacterData character)
        {
            var card = new Border
            {
                Style = (Style)FindResource("AssistantMessageBubble"),
                Margin = new Thickness(0, 0, 0, 10),
                Cursor = Cursors.Hand
            };

            card.MouseLeftButtonUp += (s, e) => SelectCharacter(character);

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // Character info
            var infoPanel = new StackPanel();
            
            var nameText = new TextBlock
            {
                Text = character.Name,
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Foreground = System.Windows.Media.Brushes.White,
                Margin = new Thickness(0, 0, 0, 5)
            };
            
            var descText = new TextBlock
            {
                Text = character.Description?.Length > 100 ? character.Description.Substring(0, 100) + "..." : character.Description,
                FontSize = 12,
                Foreground = System.Windows.Media.Brushes.LightGray,
                TextWrapping = TextWrapping.Wrap,
                Opacity = 0.8
            };

            infoPanel.Children.Add(nameText);
            infoPanel.Children.Add(descText);

            // Action buttons
            var actionPanel = new StackPanel { Orientation = Orientation.Horizontal };
            
            var editButton = new Button
            {
                Content = "✏️",
                Width = 30,
                Height = 30,
                Margin = new Thickness(0, 0, 5, 0),
                Background = System.Windows.Media.Brushes.Transparent,
                BorderThickness = new Thickness(0)
            };
            editButton.Click += (s, e) => EditCharacter(character);

            var deleteButton = new Button
            {
                Content = "🗑️",
                Width = 30,
                Height = 30,
                Background = System.Windows.Media.Brushes.Transparent,
                BorderThickness = new Thickness(0)
            };
            deleteButton.Click += (s, e) => DeleteCharacter(character);

            actionPanel.Children.Add(editButton);
            actionPanel.Children.Add(deleteButton);

            Grid.SetColumn(infoPanel, 0);
            Grid.SetColumn(actionPanel, 1);

            grid.Children.Add(infoPanel);
            grid.Children.Add(actionPanel);

            card.Child = grid;
            return card;
        }

        private void SelectCharacter(CharacterData character)
        {
            CharacterSelected?.Invoke(character);
            var parent = Parent as Window;
            parent?.Close();
        }

        private void EditCharacter(CharacterData character)
        {
            // Create edit dialog
            var dialog = new CharacterEditDialog(character);
            dialog.Owner = Window.GetWindow(this);
            
            if (dialog.ShowDialog() == true)
            {
                SaveCharacter(dialog.UpdatedCharacter);
                RefreshCharacterList();
            }
        }

        private void DeleteCharacter(CharacterData character)
        {
            var result = MessageBox.Show($"Delete character '{character.Name}'?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                Characters.Remove(character);
                
                // Delete file
                try
                {
                    var charactersPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LLMOverlay", "characters");
                    var filePath = Path.Combine(charactersPath, $"{character.Id}.json");
                    
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting character file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                
                RefreshCharacterList();
            }
        }

        private void SaveCharacter(CharacterData character)
        {
            try
            {
                var charactersPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LLMOverlay", "characters");
                Directory.CreateDirectory(charactersPath);
                
                var filePath = Path.Combine(charactersPath, $"{character.Id}.json");
                var json = JsonConvert.SerializeObject(character, Formatting.Indented);
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving character: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void NewCharacterButton_Click(object sender, RoutedEventArgs e)
        {
            var newCharacter = new CharacterData
            {
                Id = Guid.NewGuid().ToString(),
                Name = "New Character",
                Description = "Enter character description...",
                Personality = "Friendly and helpful",
                FirstMessage = "Hello! How can I help you today?"
            };

            var dialog = new CharacterEditDialog(newCharacter);
            dialog.Owner = Window.GetWindow(this);
            
            if (dialog.ShowDialog() == true)
            {
                Characters.Add(dialog.UpdatedCharacter);
                SaveCharacter(dialog.UpdatedCharacter);
                RefreshCharacterList();
            }
        }

        private void ImportCharacterButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                Title = "Import Character"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var json = File.ReadAllText(dialog.FileName);
                    var character = JsonConvert.DeserializeObject<CharacterData>(json);
                    
                    if (character != null)
                    {
                        character.Id = Guid.NewGuid().ToString(); // Generate new ID to avoid conflicts
                        Characters.Add(character);
                        SaveCharacter(character);
                        RefreshCharacterList();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error importing character: {ex.Message}", "Import Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            var parent = Parent as Window;
            parent?.Close();
        }
    }

    public class CharacterData
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string Personality { get; set; } = "";
        public string FirstMessage { get; set; } = "";
        public string Avatar { get; set; } = "";
        public Dictionary<string, string> CustomFields { get; set; } = new Dictionary<string, string>();
    }
}