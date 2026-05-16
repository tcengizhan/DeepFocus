using DeepFocus.Services;

namespace DeepFocus.ViewModels;

public sealed class ClockViewModel : BaseViewModel
{
    private readonly IGoalService _goalService;
    private string _time = DateTime.Now.ToString("HH:mm");
    private string _seconds = DateTime.Now.ToString("ss");
    private string _date = DateTime.Now.ToString("dddd, dd MMMM yyyy").ToUpperInvariant();

    public ClockViewModel(ITimerService timerService, IGoalService goalService)
    {
        _goalService = goalService;
        timerService.Tick += (_, _) => RefreshTime();
        timerService.Start();
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

    public double DailyGoalProgress => _goalService.GetDailyGoalProgress();

    public int DailyStreak => _goalService.GetDailyStreak();

    public string LongestSession => "01:45";

    public string WeeklyComparison => "+18%";

    private void RefreshTime()
    {
        var now = DateTime.Now;
        Time = now.ToString("HH:mm");
        Seconds = now.ToString("ss");
        Date = now.ToString("dddd, dd MMMM yyyy").ToUpperInvariant();
    }
}
