using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Runtime.InteropServices;

namespace LLMOverlay
{
    public sealed partial class MainWindow : Window
    {
        // Windows API for always-on-top
        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        private const int HWND_TOPMOST = -1;
        private const int SWP_NOSIZE = 0x0001;
        private const int SWP_NOMOVE = 0x0002;
        private const uint SWP_NOACTIVATE = 0x0010;

        public MainWindow()
        {
            this.InitializeComponent();
            InitializeWindow();
        }

        private void InitializeWindow()
        {
            // Set window to be always on top
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            SetWindowPos(hwnd, new IntPtr(HWND_TOPMOST), 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_NOACTIVATE);

            // Set initial window properties
            this.Title = "LLM Chat Overlay Test";
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Simple test
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Simple test
        }
    }
}