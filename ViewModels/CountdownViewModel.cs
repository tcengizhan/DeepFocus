using System.Media;
using System.Windows;
using System.Windows.Input;
using DeepFocus.Services;

namespace DeepFocus.ViewModels;

public sealed class CountdownViewModel : BaseViewModel
{
    private int _minutes = 25;
    private TimeSpan _remaining = TimeSpan.FromMinutes(25);
    private bool _isRunning;

    public CountdownViewModel(ITimerService timerService)
    {
        timerService.Tick += (_, _) => Tick();
        timerService.Start();

        StartCommand = new RelayCommand(ToggleStart);
        ResetCommand = new RelayCommand(Reset);
    }

    public int Minutes
    {
        get => _minutes;
        set
        {
            if (SetProperty(ref _minutes, Math.Max(1, value)) && !_isRunning)
            {
                Remaining = TimeSpan.FromMinutes(_minutes);
            }
        }
    }

    public TimeSpan Remaining
    {
        get => _remaining;
        private set
        {
            if (SetProperty(ref _remaining, value))
            {
                OnPropertyChanged(nameof(DisplayTime));
            }
        }
    }

    public string DisplayTime => Remaining.ToString(@"mm\:ss");

    public string StartButtonText => _isRunning ? "PAUSE" : "START";

    public ICommand StartCommand { get; }

    public ICommand ResetCommand { get; }

    private void ToggleStart()
    {
        _isRunning = !_isRunning;
        OnPropertyChanged(nameof(StartButtonText));
    }

    private void Reset()
    {
        _isRunning = false;
        Remaining = TimeSpan.FromMinutes(Minutes);
        OnPropertyChanged(nameof(StartButtonText));
    }

    private void Tick()
    {
        if (!_isRunning)
        {
            return;
        }

        Remaining = Remaining.Subtract(TimeSpan.FromSeconds(1));

        if (Remaining > TimeSpan.Zero)
        {
            return;
        }

        _isRunning = false;
        Remaining = TimeSpan.Zero;
        OnPropertyChanged(nameof(StartButtonText));
        SystemSounds.Asterisk.Play();
        MessageBox.Show("Timer tamamlandı.", "DeepFocus", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}
