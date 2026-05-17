using System.Windows.Media;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Media;
using System.IO;
using DeepFocus.Models;
using DeepFocus.Services;

namespace DeepFocus.ViewModels;

public sealed class CountdownViewModel : BaseViewModel
{
    private int _minutes = 25;
    private TimeSpan _remaining = TimeSpan.FromMinutes(25);
    private DateTime? _startedAt;
    private bool _isRunning;
    private bool _isCompletionOverlayVisible;
    private string _completedDurationText = "0 dk tamamland\u0131";
    private readonly ISessionService _sessionService;
    private SoundPlayer? _activePlayer;

    public event Action<int>? RequestTabSwitch;

    public CountdownViewModel(ITimerService timerService, ISessionService sessionService)
    {
        _sessionService = sessionService;
        _sessionService.SessionsChanged += async (_, _) => await RefreshTimerHistoryAsync();
        timerService.Tick += (_, _) => Tick();
        timerService.Start();

        StartCommand = new RelayCommand(ToggleStart);
        ResetCommand = new RelayCommand(Reset);
        DismissCompletionCommand = new RelayCommand(DismissCompletion);
        ClearHistoryCommand = new RelayCommand(() => _ = ClearHistoryAsync());
        _ = RefreshTimerHistoryAsync();


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

    public ICommand DismissCompletionCommand { get; }

    public ICommand ClearHistoryCommand { get; }

    public ObservableCollection<TimerSession> TimerHistory { get; } = [];

    public bool IsCompletionOverlayVisible
    {
        get => _isCompletionOverlayVisible;
        private set => SetProperty(ref _isCompletionOverlayVisible, value);
    }

    public string CompletedDurationText
    {
        get => _completedDurationText;
        private set => SetProperty(ref _completedDurationText, value);
    }

    private void ToggleStart()
    {
        _isRunning = !_isRunning;
        if (_isRunning)
        {
            IsCompletionOverlayVisible = false;
            _startedAt ??= DateTime.Now;
        }

        OnPropertyChanged(nameof(StartButtonText));
    }

    private void Reset()
    {
        _isRunning = false;
        _startedAt = null;
        IsCompletionOverlayVisible = false;
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

        // Switch to timer tab (index 2) first
        RequestTabSwitch?.Invoke(2);

        // Play alarm sound
        PlayAlarmSound();

        // Show completion overlay
        CompletedDurationText = $"{Minutes} dk tamamland\u0131";
        IsCompletionOverlayVisible = true;
    }

    private void PlayAlarmSound()
    {
        try
        {
            var alarmPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "alarm_sesi_Og.wav");
                
            if (File.Exists(alarmPath))
            {
                _activePlayer = new SoundPlayer(alarmPath);
                _activePlayer.Play();
            }
            else
            {
                SystemSounds.Exclamation.Play();
            }
        }
        catch
        {
            SystemSounds.Exclamation.Play();
        }
    }

    private void DismissCompletion()
    {
        IsCompletionOverlayVisible = false;
        try
        {
            _activePlayer?.Stop();
        }
        catch { }
    }

    private async Task SaveCompletedSessionAsync()
    {
        await _sessionService.AddSessionAsync(new TimerSession
        {
            StartedAt = _startedAt ?? DateTime.Now.Subtract(TimeSpan.FromMinutes(Minutes)),
            EndedAt = DateTime.Now,
            Duration = TimeSpan.FromMinutes(Minutes),
            Mode = "Zamanlayıcı",
            Completed = true
        });

        _startedAt = null;
    }

    private async Task RefreshTimerHistoryAsync()
    {
        TimerHistory.Clear();
        foreach (var session in (await _sessionService.GetRecentSessionsAsync())
            .Where(session => (session.Mode == "Zamanlayıcı" || session.Mode == "Timer") && !session.HiddenFromTimerTab))
        {
            TimerHistory.Add(session);
        }
    }

    private async Task ClearHistoryAsync()
    {
        await _sessionService.ClearTimerHistoryAsync();
    }
}
