using DeepFocus.Models;

namespace DeepFocus.Services;

public interface ISessionService
{
    event EventHandler? SessionsChanged;

    Task<IReadOnlyList<TimerSession>> GetRecentSessionsAsync(CancellationToken cancellationToken = default);

    Task AddSessionAsync(TimerSession session, CancellationToken cancellationToken = default);

    Task<double[]> GetCurrentWeekMinutesAsync(CancellationToken cancellationToken = default);

    Task<double> GetTodayWorkedMinutesAsync(CancellationToken cancellationToken = default);

    Task<int> GetDailyStreakAsync(CancellationToken cancellationToken = default);

    Task<TimeSpan> GetLongestSessionAsync(CancellationToken cancellationToken = default);

    Task<int> GetDailyGoalMinutesAsync(CancellationToken cancellationToken = default);

    Task SetDailyGoalMinutesAsync(int minutes, CancellationToken cancellationToken = default);

    Task ResetDailyGoalAsync(CancellationToken cancellationToken = default);
}
