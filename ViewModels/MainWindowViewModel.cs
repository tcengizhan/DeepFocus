using System;

namespace DeepFocus.ViewModels;

public sealed class MainWindowViewModel : BaseViewModel
{
    private int _activeTabIndex;

    public MainWindowViewModel(
        ClockViewModel clock,
        StopwatchViewModel stopwatch,
        CountdownViewModel countdown,
        StatisticsViewModel statistics)
    {
        Clock = clock;
        Stopwatch = stopwatch;
        Countdown = countdown;
        Statistics = statistics;

        Countdown.RequestTabSwitch += index => ActiveTabIndex = index;
    }

    public int ActiveTabIndex
    {
        get => _activeTabIndex;
        set => SetProperty(ref _activeTabIndex, value);
    }

    public ClockViewModel Clock { get; }

    public StopwatchViewModel Stopwatch { get; }

    public CountdownViewModel Countdown { get; }

    public StatisticsViewModel Statistics { get; }
}
