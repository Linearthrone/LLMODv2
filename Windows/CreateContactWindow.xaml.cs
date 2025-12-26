using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using LLMOverlay.Models;
using LLMOverlay.Services;

namespace LLMOverlay
{
    public partial class CreateContactWindow : Window
    {
        private Contact _contact;
        private string? _selectedAvatarPath;

        public CreateContactWindow()
        {
            InitializeComponent();
            _contact = new Contact();
            this.DataContext = _contact;
        }

        public CreateContactWindow(Contact contact) : this()
        {
            _contact = contact.Clone();
            this.DataContext = _contact;
            this.Title = "Edit Contact";
            
            // Load existing avatar if present
            if (!string.IsNullOrEmpty(_contact.AvatarPath))
            {
                LoadAvatar(_contact.AvatarPath);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateInput())
            {
                // Update contact properties
                _contact.Name = NameTextBox.Text.Trim();
                _contact.BaseModel = (BaseModelComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "gpt-3.5-turbo";
                _contact.PhysicalDescription = PhysicalDescriptionTextBox.Text.Trim();
                _contact.Personality = PersonalityTextBox.Text.Trim();
                _contact.Skills = SkillsTextBox.Text.Trim();
                _contact.SystemPrompt = SystemPromptTextBox.Text.Trim();
                _contact.AvatarPath = _selectedAvatarPath ?? string.Empty;

                try
                {
                    ContactManager.SaveContact(_contact);
                    this.DialogResult = true;
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving contact: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BrowseAvatarButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "Select Avatar Image",
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif|All Files|*.*",
                FilterIndex = 1
            };

            if (openFileDialog.ShowDialog() == true)
            {
                LoadAvatar(openFileDialog.FileName);
            }
        }

        private void RemoveAvatarButton_Click(object sender, RoutedEventArgs e)
        {
            _selectedAvatarPath = null;
            AvatarPlaceholder.Text = "👤";
            AvatarPathText.Text = "No avatar selected";
            
            // Reset avatar background
            AvatarBorder.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(64, 0, 0, 0));
        }

        private void LoadAvatar(string filePath)
        {
            try
            {
                _selectedAvatarPath = filePath;
                AvatarPathText.Text = Path.GetFileName(filePath);
                
                // Try to load the image as avatar background
                var bitmap = new System.Windows.Media.Imaging.BitmapImage(new Uri(filePath));
                var imageBrush = new System.Windows.Media.ImageBrush(bitmap);
                imageBrush.Stretch = System.Windows.Media.Stretch.UniformToFill;
                AvatarBorder.Background = imageBrush;
                AvatarPlaceholder.Text = ""; // Hide placeholder when image is loaded
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading avatar: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                _selectedAvatarPath = null;
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                MessageBox.Show("Please enter a contact name.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                NameTextBox.Focus();
                return false;
            }

            if (BaseModelComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please select a base model.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                BaseModelComboBox.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(SystemPromptTextBox.Text))
            {
                MessageBox.Show("Please enter a system prompt for the contact.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                SystemPromptTextBox.Focus();
                return false;
            }

            return true;
        }

        protected override void OnDeactivated(EventArgs e)
        {
            base.OnDeactivated(e);
            // Optionally close window when deactivated
            // this.Close();
        }
    }
}