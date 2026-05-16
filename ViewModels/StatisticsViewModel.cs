using System.Collections.ObjectModel;
using DeepFocus.Models;
using DeepFocus.Services;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;

namespace DeepFocus.ViewModels;

public sealed class StatisticsViewModel : BaseViewModel
{
    public StatisticsViewModel(ISessionService sessionService)
    {
        _ = sessionService;
        WeeklySeries =
        [
            new ColumnSeries<double>
            {
                Values = [45, 60, 20, 90, 75, 120, 50],
                Name = "Dakika"
            }
        ];

        Sessions =
        [
            new TimerSession
            {
                Id = 1,
                StartedAt = DateTime.Today.AddHours(9),
                EndedAt = DateTime.Today.AddHours(10),
                Duration = TimeSpan.FromMinutes(60),
                Mode = "Focus",
                Completed = true
            }
        ];
    }

    public ISeries[] WeeklySeries { get; }

    public ObservableCollection<TimerSession> Sessions { get; }
}
