using System.Collections.Generic;
using System.Linq;
using System.Windows;
using LLMOverlay.Models;
using LLMOverlay.Services;

namespace LLMOverlay
{
    public partial class RadialMenuWindow : Window
    {
        private readonly MainWindow? _mainWindow;
        private List<Contact> _recentContacts = new();

        public RadialMenuWindow() : this(Application.Current?.MainWindow as MainWindow)
        {
        }

        public RadialMenuWindow(MainWindow? mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            LoadRecentContacts();
        }

        public void UpdateCurrentModel()
        {
            // Current radial layout does not surface model text; stub for compatibility.
        }

        private void LoadRecentContacts()
        {
            _recentContacts = ContactManager.GetRecentContacts(2);

            if (_recentContacts.Count > 0)
            {
                LastContact1Button.ToolTip = $"Recent: {_recentContacts[0].Name}";
                LastContact1Button.IsEnabled = true;
            }
            else
            {
                LastContact1Button.ToolTip = "No recent contact";
                LastContact1Button.IsEnabled = false;
            }

            if (_recentContacts.Count > 1)
            {
                LastContact2Button.ToolTip = $"Recent: {_recentContacts[1].Name}";
                LastContact2Button.IsEnabled = true;
            }
            else
            {
                LastContact2Button.ToolTip = "No recent contact";
                LastContact2Button.IsEnabled = false;
            }
        }

        private void CenterButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void LastContact1Button_Click(object sender, RoutedEventArgs e)
        {
            ActivateRecentContact(0);
        }

        private void LastContact2Button_Click(object sender, RoutedEventArgs e)
        {
            ActivateRecentContact(1);
        }

        private void ActivateRecentContact(int index)
        {
            if (index < _recentContacts.Count)
            {
                var contact = _recentContacts[index];
                ContactManager.SetActiveContact(contact);
                _mainWindow?.LoadContact(contact);
                Close();
            }
            else
            {
                MessageBox.Show("No recent contacts available.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ContactDirectoryButton_Click(object sender, RoutedEventArgs e)
        {
            var owner = (Window?)_mainWindow ?? this;
            var directoryWindow = new ContactDirectoryWindow
            {
                Owner = owner
            };
            directoryWindow.Show();
            Close();
        }

        private void LoadModelButton_Click(object sender, RoutedEventArgs e)
        {
            var owner = (Window?)_mainWindow ?? this;
            var modelWindow = new LoadModelWindow
            {
                Owner = owner
            };
            modelWindow.Show();
            Close();
        }

        private void CreateContactButton_Click(object sender, RoutedEventArgs e)
        {
            var owner = (Window?)_mainWindow ?? this;
            var createWindow = new CreateContactWindow
            {
                Owner = owner
            };
            createWindow.Show();
            Close();
        }
    }
}