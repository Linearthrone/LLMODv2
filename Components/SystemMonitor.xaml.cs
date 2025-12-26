using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace LLMOverlay.Components
{
    public partial class SystemMonitor : UserControl
    {
        private DispatcherTimer? _updateTimer;
        private readonly LLMService _llmService;
        private DateTime _startTime = DateTime.Now;
        
        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetPhysicallyInstalledSystemMemory(out long TotalMemoryInKilobytes);

        public SystemMonitor()
        {
            InitializeComponent();
            _llmService = new LLMService();
            
            InitializeTimer();
            InitializeSystemInfo();
            
            // Initial update
            _ = UpdateSystemInfo();
        }

        private void InitializeTimer()
        {
            _updateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(2) // Update every 2 seconds
            };
            _updateTimer.Tick += async (sender, e) => await UpdateSystemInfo();
            _updateTimer.Start();
        }

        private void InitializeSystemInfo()
        {
            // OS Version
            OSTextBlock.Text = $"OS: {Environment.OSVersion}";
            
            // .NET Version
            DotNetTextBlock.Text = $".NET: {Environment.Version}";
        }

        private async Task UpdateSystemInfo()
        {
            try
            {
                // Update CPU usage
                var cpuUsage = await GetCpuUsageAsync();
                CpuProgressBar.Value = cpuUsage;
                CpuTextBlock.Text = $"{cpuUsage:F0}%";
                
                // Update Memory usage
                var memoryUsage = GetMemoryUsage();
                MemoryProgressBar.Value = memoryUsage;
                MemoryTextBlock.Text = $"{memoryUsage:F0}%";
                
                // Update service status
                await UpdateServiceStatus();
                
                // Update uptime
                var uptime = DateTime.Now - _startTime;
                UptimeTextBlock.Text = $"Uptime: {uptime.Hours:D2}:{uptime.Minutes:D2}:{uptime.Seconds:D2}";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating system info: {ex.Message}");
            }
        }

        private async Task<double> GetCpuUsageAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    var cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                    cpuCounter.NextValue(); // First call returns 0
                    System.Threading.Thread.Sleep(100); // Wait for accurate reading
                    return cpuCounter.NextValue();
                }
                catch
                {
                    // Fallback to random value if performance counters aren't available
                    var random = new Random();
                    return random.NextDouble() * 100;
                }
            });
        }

        private double GetMemoryUsage()
        {
            try
            {
                var currentProcess = Process.GetCurrentProcess();
                var memoryUsed = currentProcess.WorkingSet64;
                
                // Get total system memory
                GetPhysicallyInstalledSystemMemory(out long totalMemoryKb);
                var totalMemoryBytes = totalMemoryKb * 1024;
                
                return (double)memoryUsed / totalMemoryBytes * 100;
            }
            catch
            {
                // Fallback to .NET memory measurement
                var memoryUsed = GC.GetTotalMemory(false);
                var estimatedTotal = 8L * 1024 * 1024 * 1024; // Assume 8GB
                return (double)memoryUsed / estimatedTotal * 100;
            }
        }

        private async Task UpdateServiceStatus()
        {
            try
            {
                // Test OpenAI API connectivity
                var openaiStatus = await TestService("OpenAI");
                OpenAIStatusDot.Fill = openaiStatus ? 
                    new SolidColorBrush(Color.FromRgb(0, 255, 0)) : 
                    new SolidColorBrush(Color.FromRgb(255, 0, 0));
                
                // Test Local Model connectivity
                var localModelStatus = await TestService("Local");
                LocalModelStatusDot.Fill = localModelStatus ? 
                    new SolidColorBrush(Color.FromRgb(0, 255, 0)) : 
                    new SolidColorBrush(Color.FromRgb(255, 0, 0));
                
                // Update response time (mock for now)
                ResponseTimeText.Text = $"{new Random().Next(50, 500)} ms";
            }
            catch
            {
                // If status check fails, show offline
                OpenAIStatusDot.Fill = new SolidColorBrush(Color.FromRgb(255, 0, 0));
                LocalModelStatusDot.Fill = new SolidColorBrush(Color.FromRgb(255, 0, 0));
                ResponseTimeText.Text = "Error";
            }
        }

        private async Task<bool> TestService(string serviceName)
        {
            try
            {
                // Simple connectivity test - in real implementation, you'd ping the actual service
                await Task.Delay(100); // Simulate network delay
                
                // For demo purposes, randomly return true/false
                // In real implementation, check actual service health
                return serviceName == "OpenAI" ? true : new Random().Next(0, 2) == 1;
            }
            catch
            {
                return false;
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            _ = UpdateSystemInfo();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            _updateTimer?.Stop();
            var parent = Parent as Window;
            parent?.Close();
        }

        private void SystemMonitor_Unloaded(object sender, RoutedEventArgs e)
        {
            _updateTimer?.Stop();
        }
    }
}