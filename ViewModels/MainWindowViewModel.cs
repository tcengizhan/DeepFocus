namespace DeepFocus.ViewModels;

public sealed class MainWindowViewModel
{
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
    }

    public ClockViewModel Clock { get; }

    public StopwatchViewModel Stopwatch { get; }

    public CountdownViewModel Countdown { get; }

    public StatisticsViewModel Statistics { get; }
}
