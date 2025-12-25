using System.Collections.ObjectModel;
using System.Linq;
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

            UpdateCurrentModel();
            UpdateRecentSnippet();

            _recent.CollectionChanged += (_, __) => UpdateRecentSnippet();
            Loaded += RadialMenuWindow_Loaded;
            Deactivated += (_, __) => RadialMenuPopup.IsOpen = false;
        }

        private void RadialMenuWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var workArea = SystemParameters.WorkArea;
            Left = workArea.Left + 8;
            Top = workArea.Top + 8;
        }

        public void UpdateCurrentModel()
        {
            CurrentModelText.Text = _service.GetCurrentModel();
        }

        private void UpdateRecentSnippet()
        {
            if (_recent.Count == 0)
            {
                RecentSnippetText.Text = "No AI responses yet";
                RecentSnippetText.Opacity = 0.7;
                return;
            }

            RecentSnippetText.Text = _recent.Last();
            RecentSnippetText.Opacity = 1;
        }

        private void RadialMenuButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateCurrentModel();
            UpdateRecentSnippet();
            RadialMenuPopup.IsOpen = true;
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
            RadialMenuPopup.IsOpen = false;
            _main.ShowFromRadial(showSettingsPanel: true);
        }
    }
}
