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

    public ClockViewModel(ITimerService timerService, IGoalService goalService, ISessionService sessionService)
    {
        _goalService = goalService;
        _sessionService = sessionService;
        _sessionService.SessionsChanged += async (_, _) => await RefreshGoalProgressAsync();
        timerService.Tick += (_, _) => RefreshTime();
        timerService.Start();
        _ = RefreshGoalProgressAsync();
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

    public int DailyStreak => 4;

    public string LongestSession => "01:45";

    public string WeeklyComparison => "+18%";

    private void RefreshTime()
    {
        var now = DateTime.Now;
        Time = now.ToString("HH:mm");
        Seconds = now.ToString("ss");
        Date = now.ToString("dddd, dd MMMM yyyy").ToUpperInvariant();
    }

    private async Task RefreshGoalProgressAsync()
    {
        DailyGoalProgress = await _goalService.GetDailyGoalProgressAsync();
    }
}
