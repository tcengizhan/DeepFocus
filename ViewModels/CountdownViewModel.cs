using System.Media;
using System.Windows;
using System.Windows.Input;
using DeepFocus.Models;
using DeepFocus.Services;

namespace DeepFocus.ViewModels;

public sealed class CountdownViewModel : BaseViewModel
{
    private int _minutes = 25;
    private TimeSpan _remaining = TimeSpan.FromMinutes(25);
    private DateTime? _startedAt;
    private bool _isRunning;
    private readonly ISessionService _sessionService;

    public CountdownViewModel(ITimerService timerService, ISessionService sessionService)
    {
        _sessionService = sessionService;
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

    public string DisplayTime => $"{(int)Remaining.TotalMinutes:00}:{Remaining.Seconds:00}";

    public string StartButtonText => _isRunning ? "PAUSE" : "START";

    public ICommand StartCommand { get; }

    public ICommand ResetCommand { get; }

    private void ToggleStart()
    {
        _isRunning = !_isRunning;
        if (_isRunning)
        {
            _startedAt ??= DateTime.Now;
        }

        OnPropertyChanged(nameof(StartButtonText));
    }

    private void Reset()
    {
        _isRunning = false;
        _startedAt = null;
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
        _ = SaveCompletedSessionAsync();
        OnPropertyChanged(nameof(StartButtonText));
        SystemSounds.Asterisk.Play();
        MessageBox.Show("Timer tamamlandi.", "DeepFocus", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private async Task SaveCompletedSessionAsync()
    {
        await _sessionService.AddSessionAsync(new TimerSession
        {
            StartedAt = _startedAt ?? DateTime.Now.Subtract(TimeSpan.FromMinutes(Minutes)),
            EndedAt = DateTime.Now,
            Duration = TimeSpan.FromMinutes(Minutes),
            Mode = "Timer",
            Completed = true
        });

        _startedAt = null;
    }
}
