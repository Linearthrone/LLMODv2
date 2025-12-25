using System;
using System.Linq;
using System.Windows;

namespace LLMOverlay.Components
{
    public partial class LoreItemDialog : Window
    {
        public LoreItem LoreItem { get; private set; }
        
        public LoreItemDialog(LoreItem existingLoreItem = null)
        {
            InitializeComponent();
            
            if (existingLoreItem != null)
            {
                LoreItem = new LoreItem
                {
                    Id = existingLoreItem.Id,
                    Title = existingLoreItem.Title,
                    Content = existingLoreItem.Content,
                    Tags = existingLoreItem.Tags.ToList(),
                    CreatedAt = existingLoreItem.CreatedAt,
                    UpdatedAt = DateTime.Now
                };
            }
            else
            {
                LoreItem = new LoreItem();
            }
            
            LoadLoreItemData();
        }

        private void LoadLoreItemData()
        {
            TitleTextBox.Text = LoreItem.Title ?? "";
            ContentTextBox.Text = LoreItem.Content ?? "";
            TagsTextBox.Text = string.Join(", ", LoreItem.Tags);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            LoreItem.Title = TitleTextBox.Text?.Trim();
            LoreItem.Content = ContentTextBox.Text?.Trim();
            LoreItem.Tags = TagsTextBox.Text?.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                      .Select(t => t.Trim())
                                      .Where(t => !string.IsNullOrEmpty(t))
                                      .ToList() ?? new List<string>();
            LoreItem.UpdatedAt = DateTime.Now;
            
            if (string.IsNullOrWhiteSpace(LoreItem.Title))
            {
                MessageBox.Show("Title is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            if (string.IsNullOrWhiteSpace(LoreItem.Content))
            {
                MessageBox.Show("Content is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            this.DialogResult = true;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}