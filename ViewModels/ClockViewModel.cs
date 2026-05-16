using DeepFocus.Services;

namespace DeepFocus.ViewModels;

public sealed class ClockViewModel : BaseViewModel
{
    private readonly IGoalService _goalService;
    private readonly ISessionService _sessionService;
    private string _time = DateTime.Now.ToString("HH:mm");
    private string _seconds = DateTime.Now.ToString("ss");
    private string _date = DateTime.Now.ToString("dddd, dd MMMM yyyy").ToUpperInvariant();
    private double _dailyGoalProgress;
    private double _goalDotOffset;
    private string _dailyGoalPercentText = "%0";
    private string _dailyStreakDisplay = "0";
    private string _longestSession = "00:00";
    private string _weeklyComparison = "%0";

    public ClockViewModel(ITimerService timerService, IGoalService goalService, ISessionService sessionService)
    {
        _goalService = goalService;
        _sessionService = sessionService;
        _sessionService.SessionsChanged += async (_, _) => await RefreshDashboardAsync();
        timerService.Tick += (_, _) => RefreshTime();
        timerService.Start();
        _ = RefreshDashboardAsync();
    }

    public string Time
    {
        get => _time;
        private set => SetProperty(ref _time, value);
    }

    public string Seconds
    {
        get => _seconds;
        private set => SetProperty(ref _seconds, value);
    }

    public string Date
    {
        get => _date;
        private set => SetProperty(ref _date, value);
    }

    public double DailyGoalProgress
    {
        get => _dailyGoalProgress;
        private set
        {
            if (SetProperty(ref _dailyGoalProgress, value))
            {
                GoalDotOffset = Math.Clamp(value, 0, 1) * 294;
            }
        }
    }

    public double GoalDotOffset
    {
        get => _goalDotOffset;
        private set => SetProperty(ref _goalDotOffset, value);
    }

    public string DailyGoalPercentText
    {
        get => _dailyGoalPercentText;
        private set => SetProperty(ref _dailyGoalPercentText, value);
    }

    public string DailyStreakDisplay
    {
        get => _dailyStreakDisplay;
        private set => SetProperty(ref _dailyStreakDisplay, value);
    }

    public string LongestSession
    {
        get => _longestSession;
        private set => SetProperty(ref _longestSession, value);
    }

    public string WeeklyComparison
    {
        get => _weeklyComparison;
        private set => SetProperty(ref _weeklyComparison, value);
    }

    private void RefreshTime()
    {
        var now = DateTime.Now;
        Time = now.ToString("HH:mm");
        Seconds = now.ToString("ss");
        Date = now.ToString("dddd, dd MMMM yyyy").ToUpperInvariant();
    }

    private async Task RefreshDashboardAsync()
    {
        DailyGoalProgress = await _goalService.GetDailyGoalProgressAsync();
        DailyGoalPercentText = $"%{Math.Round(DailyGoalProgress * 100)}";

        var streak = await _goalService.GetDailyStreakAsync();
        DailyStreakDisplay = streak.ToString();

        var longest = await _sessionService.GetLongestSessionAsync();
        LongestSession = longest.TotalHours >= 1
            ? longest.ToString(@"hh\:mm\:ss")
            : longest.ToString(@"mm\:ss");

        var week = await _sessionService.GetCurrentWeekMinutesAsync();
        WeeklyComparison = $"{Math.Round(week.Sum())} dk";
    }
}
