using System.Windows;
using System.Windows.Media;
using TaskMaster.Host.Services;

namespace TaskMaster.Host;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly ServiceManager _serviceManager;
    private System.Windows.Threading.DispatcherTimer? _statusTimer;

    public MainWindow()
    {
        InitializeComponent();
        _serviceManager = new ServiceManager();
        
        // Setup status update timer
        _statusTimer = new System.Windows.Threading.DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _statusTimer.Tick += StatusTimer_Tick;
        _statusTimer.Start();

        // Handle window closing
        Closing += MainWindow_Closing;
    }

    private async void StartButton_Click(object sender, RoutedEventArgs e)
    {
        StartButton.IsEnabled = false;
        StopButton.IsEnabled = true;

        try
        {
            await _serviceManager.StartAllAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao iniciar serviços: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            StartButton.IsEnabled = true;
            StopButton.IsEnabled = false;
        }
    }

    private void StopButton_Click(object sender, RoutedEventArgs e)
    {
        _serviceManager.StopAll();
        StartButton.IsEnabled = true;
        StopButton.IsEnabled = false;
    }

    private void StatusTimer_Tick(object? sender, EventArgs e)
    {
        UpdateStatus();
    }

    private void UpdateStatus()
    {
        // Update API status
        if (_serviceManager.IsApiRunning)
        {
            ApiStatusIndicator.Fill = new SolidColorBrush(Colors.Green);
            ApiStatusText.Text = "Running";
        }
        else
        {
            ApiStatusIndicator.Fill = new SolidColorBrush(Colors.Gray);
            ApiStatusText.Text = "Stopped";
        }

        // Update Worker status
        if (_serviceManager.IsWorkerRunning)
        {
            WorkerStatusIndicator.Fill = new SolidColorBrush(Colors.Green);
            WorkerStatusText.Text = "Running";
        }
        else
        {
            WorkerStatusIndicator.Fill = new SolidColorBrush(Colors.Gray);
            WorkerStatusText.Text = "Stopped";
        }

        // Update Blazor status
        if (_serviceManager.IsBlazorRunning)
        {
            BlazorStatusIndicator.Fill = new SolidColorBrush(Colors.Green);
            BlazorStatusText.Text = "Running";
        }
        else
        {
            BlazorStatusIndicator.Fill = new SolidColorBrush(Colors.Gray);
            BlazorStatusText.Text = "Stopped";
        }
    }

    private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        _statusTimer?.Stop();
        _serviceManager.StopAll();
        _serviceManager.Dispose();
    }
}
