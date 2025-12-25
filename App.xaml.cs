using System;
using System.Windows;
using System.Windows.Controls;

namespace LLMOverlay
{
    public partial class App : Application
    {
        public Window? MainWindow { get; private set; }

        public App()
        {
            this.InitializeComponent();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            MainWindow = new MainWindow();
            MainWindow.Show();
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            if (MainWindow == null)
            {
                MainWindow = new MainWindow();
                MainWindow.Show();
            }
        }
    }
}