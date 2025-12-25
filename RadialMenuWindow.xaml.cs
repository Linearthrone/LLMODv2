using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace LLMOverlay
{
    public partial class RadialMenuWindow : Window
    {
        private readonly MainWindow _main;
        private readonly LLMService _service;
        private readonly ObservableCollection<string> _recent;

        public RadialMenuWindow(MainWindow main, LLMService service, ObservableCollection<string> recent)
        {
            InitializeComponent();
            _main = main;
            _service = service;
            _recent = recent;

            RecentContactsList.ItemsSource = _recent;
            UpdateCurrentModel();
            UpdateContactsVisibility();

            _recent.CollectionChanged += (_, __) => UpdateContactsVisibility();
            Loaded += RadialMenuWindow_Loaded;
            Deactivated += (_, __) => RadialMenuPopup.IsOpen = false;
        }

        private void RadialMenuWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var workArea = SystemParameters.WorkArea;
            Left = workArea.Left + 8;
            Top = workArea.Top + 8;
        }

        private void UpdateContactsVisibility()
        {
            NoContactsText.Visibility = _recent.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        public void UpdateCurrentModel()
        {
            CurrentModelText.Text = _service.GetCurrentModel();
        }

        private void RadialMenuButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateCurrentModel();
            UpdateContactsVisibility();
            RadialMenuPopup.IsOpen = true;
        }

        private void LoadModelButton_Click(object sender, RoutedEventArgs e)
        {
            RadialMenuPopup.IsOpen = false;
            _main.ShowFromRadial(showSettingsPanel: true);
        }

        private void CreatePersonaButton_Click(object sender, RoutedEventArgs e)
        {
            RadialMenuPopup.IsOpen = false;
            _main.ShowFromRadial(showSettingsPanel: false);
            _main.ShowPersonaComingSoon();
        }
    }
}
