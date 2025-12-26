using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace LLMOverlay
{
    public partial class RadialMenuWindow : Window
    {
        private MainWindow _mainWindow;
        private List<Contact> _recentContacts;

        public RadialMenuWindow()
        {
            InitializeComponent();
        }

        private void LoadRecentContacts()
        {
            // Load recent contacts from storage
            // For now, we'll use placeholder data
            _recentContacts = ContactManager.GetRecentContacts(2);
            
            // Update button tooltips with contact names
            if (_recentContacts.Count > 0)
            {
                LastContact1Button.ToolTip = $"Recent: {_recentContacts[0].Name}";
            }
            
            if (_recentContacts.Count > 1)
            {
                LastContact2Button.ToolTip = $"Recent: {_recentContacts[1].Name}";
            }
        }

        {
            if (_recentContacts.Count > 0)
            {
                var contact = _recentContacts[0];
                ContactManager.SetActiveContact(contact);
                _mainWindow?.LoadContact(contact);
                this.Close();
            }
            else
            {
                MessageBox.Show("No recent contacts available.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        }

        private void OpenChatButton_Click(object sender, RoutedEventArgs e)
        {
            RadialMenuPopup.IsOpen = false;
            _main.ShowFromRadial(showSettingsPanel: false);
        }

        private void OpenCharacterButton_Click(object sender, RoutedEventArgs e)
        {
            RadialMenuPopup.IsOpen = false;
            _main.OpenCharacterManager();
        }

        private void OpenWorldInfoButton_Click(object sender, RoutedEventArgs e)
        {
            RadialMenuPopup.IsOpen = false;
            _main.OpenWorldInfo();
        }

        private void OpenSystemMonitorButton_Click(object sender, RoutedEventArgs e)
        {
            RadialMenuPopup.IsOpen = false;
            _main.OpenSystemMonitor();
        }

        private void OpenSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var modelWindow = new LoadModelWindow();
            modelWindow.Owner = this;
            modelWindow.ShowDialog();
            this.Close();
        }
    }
}