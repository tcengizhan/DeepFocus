using System.Collections.ObjectModel;
using DeepFocus.Models;
using DeepFocus.Services;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System.Windows.Input;

namespace DeepFocus.ViewModels;

public sealed class StatisticsViewModel : BaseViewModel
{
    private readonly ISessionService _sessionService;
    private int _dailyGoalHours;
    private string _dailyGoalHoursInput = "0";
    private bool _isSaveToastVisible;
    private int _saveToastVersion;
    private string _saveToastMessage = "Hedef kaydedildi \u2713";
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
                Name = "Dakika",
                Fill = new SolidColorPaint(SKColors.White),
                Stroke = null,
                MaxBarWidth = 18
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
        TodayTimerSessions = [];
        SaveDailyGoalCommand = new RelayCommand(() => _ = SaveDailyGoalAsync());
        ResetDailyGoalCommand = new RelayCommand(() => _ = ResetDailyGoalAsync());
        _ = RefreshAsync();
    }

    public ISeries[] WeeklySeries { get; }

    public Axis[] XAxes { get; }

    public Axis[] YAxes { get; }

    public ObservableCollection<TimerSession> Sessions { get; }

    public ObservableCollection<TimerSession> TodayTimerSessions { get; }

    public ICommand SaveDailyGoalCommand { get; }

    public ICommand ResetDailyGoalCommand { get; }

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
            var sanitized = Math.Max(0, value);
            if (SetProperty(ref _dailyGoalHours, sanitized))
            {
                _ = _sessionService.SetDailyGoalMinutesAsync(sanitized * 60);
            }
        }
    }

    public string DailyGoalHoursInput
    {
        get => _dailyGoalHoursInput;
        set => SetProperty(ref _dailyGoalHoursInput, value);
    }

    public bool IsSaveToastVisible
    {
        get => _isSaveToastVisible;
        private set => SetProperty(ref _isSaveToastVisible, value);
    }

    public string SaveToastMessage
    {
        get => _saveToastMessage;
        private set => SetProperty(ref _saveToastMessage, value);
    }

    private async Task RefreshAsync()
    {
        var goalMinutes = await _sessionService.GetDailyGoalMinutesAsync();
        _dailyGoalHours = goalMinutes <= 0 ? 0 : Math.Max(1, (int)Math.Ceiling(goalMinutes / 60.0));
        _dailyGoalHoursInput = _dailyGoalHours.ToString();
        OnPropertyChanged(nameof(DailyGoalHours));
        OnPropertyChanged(nameof(DailyGoalHoursInput));

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

        TodayTimerSessions.Clear();
        foreach (var session in await _sessionService.GetTodayTimerSessionsAsync())
        {
            TodayTimerSessions.Add(session);
        }
    }

    private async Task ResetDailyGoalAsync()
    {
        await _sessionService.ResetDailyGoalAsync();
        _dailyGoalHours = 0;
        DailyGoalHoursInput = "0";
        OnPropertyChanged(nameof(DailyGoalHours));
        await ShowToastAsync("Hedef s\u0131f\u0131rland\u0131 \u2713");
    }

    private async Task SaveDailyGoalAsync()
    {
        if (!int.TryParse(DailyGoalHoursInput, out var hours))
        {
            hours = 0;
        }

        _dailyGoalHours = Math.Max(0, hours);
        OnPropertyChanged(nameof(DailyGoalHours));
        await _sessionService.SetDailyGoalMinutesAsync(_dailyGoalHours * 60);
        await ShowToastAsync("Hedef kaydedildi \u2713");
    }

    private async Task ShowToastAsync(string message)
    {
        var version = ++_saveToastVersion;
        SaveToastMessage = message;
        IsSaveToastVisible = true;
        await Task.Delay(TimeSpan.FromSeconds(2));

        if (version == _saveToastVersion)
        {
            IsSaveToastVisible = false;
        }
    }
}
