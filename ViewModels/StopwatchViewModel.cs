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

    public string StartButtonText => _stopwatch.IsRunning ? "STOP" : "START";

    private void ToggleStart()
    {
        if (_stopwatch.IsRunning)
        {
            _stopwatch.Stop();
            _displayTimer.Stop();
            _ = SaveSessionAsync();
        }
        else
        {
            _startedAt ??= DateTime.Now;
            _stopwatch.Start();
            _displayTimer.Start();
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
}
