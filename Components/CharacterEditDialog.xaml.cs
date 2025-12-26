using System.Windows;

namespace LLMOverlay.Components
{
    public partial class CharacterEditDialog : Window
    {
        public CharacterData UpdatedCharacter { get; private set; }
        
        public CharacterEditDialog(CharacterData character)
        {
            InitializeComponent();
            UpdatedCharacter = new CharacterData
            {
                Id = character.Id,
                Name = character.Name,
                Description = character.Description,
                Personality = character.Personality,
                FirstMessage = character.FirstMessage,
                Avatar = character.Avatar,
                CustomFields = character.CustomFields
            };
            
            LoadCharacterData();
        }

        private void LoadCharacterData()
        {
            NameTextBox.Text = UpdatedCharacter.Name ?? "";
            DescriptionTextBox.Text = UpdatedCharacter.Description ?? "";
            PersonalityTextBox.Text = UpdatedCharacter.Personality ?? "";
            FirstMessageTextBox.Text = UpdatedCharacter.FirstMessage ?? "";
            AvatarTextBox.Text = UpdatedCharacter.Avatar ?? "";
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            UpdatedCharacter.Name = NameTextBox.Text?.Trim();
            UpdatedCharacter.Description = DescriptionTextBox.Text?.Trim();
            UpdatedCharacter.Personality = PersonalityTextBox.Text?.Trim();
            UpdatedCharacter.FirstMessage = FirstMessageTextBox.Text?.Trim();
            UpdatedCharacter.Avatar = AvatarTextBox.Text?.Trim();
            
            if (string.IsNullOrWhiteSpace(UpdatedCharacter.Name))
            {
                MessageBox.Show("Character name is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
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