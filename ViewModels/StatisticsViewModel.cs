using System.Collections.ObjectModel;
using DeepFocus.Models;
using DeepFocus.Services;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace DeepFocus.ViewModels;

public sealed class StatisticsViewModel : BaseViewModel
{
    private readonly ISessionService _sessionService;
    private int _dailyGoalHours = 2;
    private ObservableCollection<double> _weeklyMinutes = [];

    public StatisticsViewModel(ISessionService sessionService)
    {
        _sessionService = sessionService;
        _sessionService.SessionsChanged += async (_, _) => await RefreshAsync();

        WeeklyMinutes = [0, 0, 0, 0, 0, 0, 0];
        WeeklySeries =
        [
            new ColumnSeries<double>
            {
                Values = WeeklyMinutes,
                Name = "Dakika"
            }
        ];

        XAxes =
        [
            new Axis
            {
                Labels = ["Pzt", "Sal", "\u00C7ar", "Per", "Cum", "Cmt", "Paz"],
                LabelsPaint = new SolidColorPaint(new SKColor(255, 255, 255, 160))
            }
        ];

        YAxes =
        [
            new Axis
            {
                Name = "Dakika",
                NamePaint = new SolidColorPaint(new SKColor(255, 255, 255, 180)),
                LabelsPaint = new SolidColorPaint(new SKColor(255, 255, 255, 160)),
                MinLimit = 0
            }
        ];

        Sessions = [];
        _ = RefreshAsync();
    }

    public ISeries[] WeeklySeries { get; }

    public Axis[] XAxes { get; }

    public Axis[] YAxes { get; }

    public ObservableCollection<TimerSession> Sessions { get; }

    public ObservableCollection<double> WeeklyMinutes
    {
        get => _weeklyMinutes;
        private set => SetProperty(ref _weeklyMinutes, value);
    }

    public int DailyGoalHours
    {
        get => _dailyGoalHours;
        set
        {
            var sanitized = Math.Max(1, value);
            if (SetProperty(ref _dailyGoalHours, sanitized))
            {
                _ = _sessionService.SetDailyGoalMinutesAsync(sanitized * 60);
            }
        }
    }

    private async Task RefreshAsync()
    {
        var goalMinutes = await _sessionService.GetDailyGoalMinutesAsync();
        _dailyGoalHours = Math.Max(1, (int)Math.Ceiling(goalMinutes / 60.0));
        OnPropertyChanged(nameof(DailyGoalHours));

        var week = await _sessionService.GetCurrentWeekMinutesAsync();
        WeeklyMinutes.Clear();
        foreach (var day in week)
        {
            WeeklyMinutes.Add(Math.Round(day, 1));
        }

        Sessions.Clear();
        foreach (var session in await _sessionService.GetRecentSessionsAsync())
        {
            Sessions.Add(session);
        }
    }
}
