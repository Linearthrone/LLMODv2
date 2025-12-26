using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json;

namespace LLMOverlay.Components
{
    public partial class WorldInfo : UserControl
    {
        public ObservableCollection<LoreItem> LoreItems { get; set; }
        public ObservableCollection<CharacterNote> CharacterNotes { get; set; }
        public WorldSettings WorldSettings { get; set; }
        
        public WorldInfo()
        {
            InitializeComponent();
            
            LoreItems = new ObservableCollection<LoreItem>();
            CharacterNotes = new ObservableCollection<CharacterNote>();
            WorldSettings = new WorldSettings();
            
            DataContext = this;
            
            LoadWorldData();
            RefreshUI();
        }

        private void LoadWorldData()
        {
            try
            {
                var worldDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LLMOverlay", "worldinfo");
                
                if (Directory.Exists(worldDataPath))
                {
                    // Load lore items
                    var lorePath = Path.Combine(worldDataPath, "lore.json");
                    if (File.Exists(lorePath))
                    {
                        var loreJson = File.ReadAllText(lorePath);
                        var loreList = JsonConvert.DeserializeObject<List<LoreItem>>(loreJson);
                        if (loreList != null)
                        {
                            foreach (var item in loreList)
                                LoreItems.Add(item);
                        }
                    }
                    
                    // Load character notes
                    var notesPath = Path.Combine(worldDataPath, "notes.json");
                    if (File.Exists(notesPath))
                    {
                        var notesJson = File.ReadAllText(notesPath);
                        var notesList = JsonConvert.DeserializeObject<List<CharacterNote>>(notesJson);
                        if (notesList != null)
                        {
                            foreach (var note in notesList)
                                CharacterNotes.Add(note);
                        }
                    }
                    
                    // Load world settings
                    var settingsPath = Path.Combine(worldDataPath, "settings.json");
                    if (File.Exists(settingsPath))
                    {
                        var settingsJson = File.ReadAllText(settingsPath);
                        var settings = JsonConvert.DeserializeObject<WorldSettings>(settingsJson);
                        if (settings != null)
                        {
                            WorldSettings = settings;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading world data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshUI()
        {
            RefreshLoreItems();
            RefreshCharacterNotes();
            RefreshWorldSettings();
        }

        private void RefreshLoreItems()
        {
            LoreItemsPanel.Children.Clear();
            
            foreach (var loreItem in LoreItems)
            {
                var loreCard = CreateLoreItemCard(loreItem);
                LoreItemsPanel.Children.Add(loreCard);
            }
        }

        private void RefreshCharacterNotes()
        {
            CharacterNotesPanel.Children.Clear();
            
            var currentCharacter = CharacterComboBox.SelectedItem as string;
            var filteredNotes = CharacterNotes.Where(n => n.CharacterName == currentCharacter).ToList();
            
            foreach (var note in filteredNotes)
            {
                var noteCard = CreateCharacterNoteCard(note);
                CharacterNotesPanel.Children.Add(noteCard);
            }
        }

        private void RefreshWorldSettings()
        {
            WorldNameTextBox.Text = WorldSettings.Name ?? "";
            WorldDescriptionTextBox.Text = WorldSettings.Description ?? "";
            TimePeriodTextBox.Text = WorldSettings.TimePeriod ?? "";
            LocationTextBox.Text = WorldSettings.Location ?? "";
        }

        private Border CreateLoreItemCard(LoreItem loreItem)
        {
            var card = new Border
            {
                Style = (Style)FindResource("SystemMessageBubble"),
                Margin = new Thickness(0, 0, 0, 10)
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var infoPanel = new StackPanel();
            
            var titleText = new TextBlock
            {
                Text = loreItem.Title,
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Foreground = System.Windows.Media.Brushes.White,
                Margin = new Thickness(0, 0, 0, 5)
            };
            
            var contentText = new TextBlock
            {
                Text = loreItem.Content?.Length > 200 ? loreItem.Content.Substring(0, 200) + "..." : loreItem.Content,
                FontSize = 12,
                Foreground = System.Windows.Media.Brushes.LightGray,
                TextWrapping = TextWrapping.Wrap
            };

            var tagsText = new TextBlock
            {
                Text = $"Tags: {string.Join(", ", loreItem.Tags)}",
                FontSize = 10,
                Foreground = System.Windows.Media.Brushes.Gray,
                FontStyle = FontStyles.Italic,
                Margin = new Thickness(0, 5, 0, 0)
            };

            infoPanel.Children.Add(titleText);
            infoPanel.Children.Add(contentText);
            infoPanel.Children.Add(tagsText);

            var actionPanel = new StackPanel { Orientation = Orientation.Horizontal };
            
            var editButton = new Button
            {
                Content = "✏️",
                Width = 25,
                Height = 25,
                Margin = new Thickness(0, 0, 5, 0),
                Background = System.Windows.Media.Brushes.Transparent,
                BorderThickness = new Thickness(0)
            };
            editButton.Click += (s, e) => EditLoreItem(loreItem);

            var deleteButton = new Button
            {
                Content = "🗑️",
                Width = 25,
                Height = 25,
                Background = System.Windows.Media.Brushes.Transparent,
                BorderThickness = new Thickness(0)
            };
            deleteButton.Click += (s, e) => DeleteLoreItem(loreItem);

            actionPanel.Children.Add(editButton);
            actionPanel.Children.Add(deleteButton);

            Grid.SetColumn(infoPanel, 0);
            Grid.SetColumn(actionPanel, 1);

            grid.Children.Add(infoPanel);
            grid.Children.Add(actionPanel);

            card.Child = grid;
            return card;
        }

        private Border CreateCharacterNoteCard(CharacterNote note)
        {
            var card = new Border
            {
                Style = (Style)FindResource("SystemMessageBubble"),
                Margin = new Thickness(0, 0, 0, 10)
            };

            var stackPanel = new StackPanel();
            
            var noteText = new TextBlock
            {
                Text = note.Content,
                FontSize = 12,
                Foreground = System.Windows.Media.Brushes.White,
                TextWrapping = TextWrapping.Wrap
            };

            var timestampText = new TextBlock
            {
                Text = note.CreatedAt.ToString("yyyy-MM-dd HH:mm"),
                FontSize = 10,
                Foreground = System.Windows.Media.Brushes.Gray,
                FontStyle = FontStyles.Italic,
                Margin = new Thickness(0, 5, 0, 0)
            };

            stackPanel.Children.Add(noteText);
            stackPanel.Children.Add(timestampText);

            card.Child = stackPanel;
            return card;
        }

        private void AddLoreButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new LoreItemDialog();
            dialog.Owner = Window.GetWindow(this);
            
            if (dialog.ShowDialog() == true)
            {
                LoreItems.Add(dialog.LoreItem);
                SaveLoreItems();
                RefreshLoreItems();
            }
        }

        private void EditLoreItem(LoreItem loreItem)
        {
            var dialog = new LoreItemDialog(loreItem);
            dialog.Owner = Window.GetWindow(this);
            
            if (dialog.ShowDialog() == true)
            {
                var index = LoreItems.IndexOf(loreItem);
                if (index >= 0)
                {
                    LoreItems[index] = dialog.LoreItem;
                    SaveLoreItems();
                    RefreshLoreItems();
                }
            }
        }

        private void DeleteLoreItem(LoreItem loreItem)
        {
            var result = MessageBox.Show($"Delete lore item '{loreItem.Title}'?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                LoreItems.Remove(loreItem);
                SaveLoreItems();
                RefreshLoreItems();
            }
        }

        private void SaveLoreItems()
        {
            try
            {
                var worldDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LLMOverlay", "worldinfo");
                Directory.CreateDirectory(worldDataPath);
                
                var lorePath = Path.Combine(worldDataPath, "lore.json");
                var json = JsonConvert.SerializeObject(LoreItems.ToList(), Formatting.Indented);
                File.WriteAllText(lorePath, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving lore items: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveWorldSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            WorldSettings.Name = WorldNameTextBox.Text?.Trim();
            WorldSettings.Description = WorldDescriptionTextBox.Text?.Trim();
            WorldSettings.TimePeriod = TimePeriodTextBox.Text?.Trim();
            WorldSettings.Location = LocationTextBox.Text?.Trim();
            
            SaveWorldSettings();
            MessageBox.Show("World settings saved!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SaveWorldSettings()
        {
            try
            {
                var worldDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LLMOverlay", "worldinfo");
                Directory.CreateDirectory(worldDataPath);
                
                var settingsPath = Path.Combine(worldDataPath, "settings.json");
                var json = JsonConvert.SerializeObject(WorldSettings, Formatting.Indented);
                File.WriteAllText(settingsPath, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving world settings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            var parent = Parent as Window;
            parent?.Close();
        }
    }

    public class LoreItem
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Title { get; set; } = "";
        public string Content { get; set; } = "";
        public List<string> Tags { get; set; } = new List<string>();
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }

    public class CharacterNote
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string CharacterName { get; set; } = "";
        public string Content { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    public class WorldSettings
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string TimePeriod { get; set; } = "";
        public string Location { get; set; } = "";
        public Dictionary<string, object> CustomSettings { get; set; } = new Dictionary<string, object>();
    }
}