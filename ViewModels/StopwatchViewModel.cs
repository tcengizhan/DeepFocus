using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows.Threading;
using DeepFocus.Models;
using DeepFocus.Services;

namespace DeepFocus.ViewModels;

public sealed class StopwatchViewModel : BaseViewModel
{
    private readonly Stopwatch _stopwatch = new();
    private readonly ISessionService _sessionService;
    private readonly DispatcherTimer _displayTimer;
    private string _displayTime = "00:00.00";
    private DateTime? _startedAt;
    private bool _isAddToWorkVisible;
    private bool _isAddToWorkToastVisible;
    private int _addToWorkToastVersion;

    public StopwatchViewModel(ISessionService sessionService)
    {
        _sessionService = sessionService;
        _displayTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(10)
        };
        _displayTimer.Tick += (_, _) => RefreshDisplay();

        StartCommand = new RelayCommand(ToggleStart);
        LapCommand = new RelayCommand(AddLap, () => _stopwatch.IsRunning);
        ResetCommand = new RelayCommand(Reset);
        AddToWorkCommand = new RelayCommand(() => _ = AddToWorkAsync());
    }

    public string DisplayTime
    {
        get => _displayTime;
        private set => SetProperty(ref _displayTime, value);
    }

    public ObservableCollection<LapRecord> Laps { get; } = [];

    public ICommand StartCommand { get; }

    public ICommand LapCommand { get; }

    public ICommand ResetCommand { get; }

    public ICommand AddToWorkCommand { get; }

    public string StartButtonText => _stopwatch.IsRunning ? "STOP" : "START";

    public bool IsAddToWorkVisible
    {
        get => _isAddToWorkVisible;
        private set => SetProperty(ref _isAddToWorkVisible, value);
    }

    public bool IsAddToWorkToastVisible
    {
        get => _isAddToWorkToastVisible;
        private set => SetProperty(ref _isAddToWorkToastVisible, value);
    }

    private void ToggleStart()
    {
        if (_stopwatch.IsRunning)
        {
            _stopwatch.Stop();
            _displayTimer.Stop();
            _ = SaveSessionAsync();
            IsAddToWorkVisible = _stopwatch.Elapsed >= TimeSpan.FromSeconds(1);
        }
        else
        {
            _startedAt ??= DateTime.Now;
            _stopwatch.Start();
            _displayTimer.Start();
            IsAddToWorkVisible = false;
            IsAddToWorkToastVisible = false;
        }

        OnPropertyChanged(nameof(StartButtonText));
        (LapCommand as RelayCommand)?.RaiseCanExecuteChanged();
    }

    private void AddLap()
    {
        Laps.Insert(0, new LapRecord
        {
            Number = Laps.Count + 1,
            Elapsed = _stopwatch.Elapsed
        });
    }

    private void Reset()
    {
        _stopwatch.Reset();
        _displayTimer.Stop();
        _startedAt = null;
        Laps.Clear();
        IsAddToWorkVisible = false;
        IsAddToWorkToastVisible = false;
        RefreshDisplay();
        OnPropertyChanged(nameof(StartButtonText));
        (LapCommand as RelayCommand)?.RaiseCanExecuteChanged();
    }

    private void RefreshDisplay()
    {
        DisplayTime = _stopwatch.Elapsed.ToString(@"mm\:ss\.ff");
    }

    private async Task SaveSessionAsync()
    {
        if (_startedAt is null)
        {
            return;
        }

        var endedAt = DateTime.Now;
        var duration = endedAt - _startedAt.Value;
        if (duration < TimeSpan.FromSeconds(1))
        {
            return;
        }

        await _sessionService.AddSessionAsync(new TimerSession
        {
            StartedAt = _startedAt.Value,
            EndedAt = endedAt,
            Duration = duration,
            Mode = "Kronometre",
            Completed = true
        });

        _startedAt = null;
    }

    private async Task AddToWorkAsync()
    {
        var minutes = _stopwatch.Elapsed.TotalMinutes;
        if (minutes <= 0)
        {
            return;
        }

        await _sessionService.AddWorkedMinutesAsync(minutes);
        IsAddToWorkVisible = false;
        await ShowAddToWorkToastAsync();
    }

    private async Task ShowAddToWorkToastAsync()
    {
        var version = ++_addToWorkToastVersion;
        IsAddToWorkToastVisible = true;
        await Task.Delay(TimeSpan.FromSeconds(2));

        if (version == _addToWorkToastVersion)
        {
            IsAddToWorkToastVisible = false;
        }
    }
}
