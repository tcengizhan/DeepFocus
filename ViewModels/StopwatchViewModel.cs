using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows.Threading;
using DeepFocus.Models;

namespace DeepFocus.ViewModels;

public sealed class StopwatchViewModel : BaseViewModel
{
    private readonly Stopwatch _stopwatch = new();
    private readonly DispatcherTimer _displayTimer;
    private string _displayTime = "00:00.00";

    public StopwatchViewModel()
    {
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
        }
        else
        {
            _stopwatch.Start();
            _displayTimer.Start();
        }

        if (!_stopwatch.IsRunning)
        {
            _displayTimer.Stop();
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
        Laps.Clear();
        RefreshDisplay();
        OnPropertyChanged(nameof(StartButtonText));
        (LapCommand as RelayCommand)?.RaiseCanExecuteChanged();
    }

    private void RefreshDisplay()
    {
        DisplayTime = _stopwatch.Elapsed.ToString(@"mm\:ss\.ff");
    }
}
