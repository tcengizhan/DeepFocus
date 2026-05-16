namespace DeepFocus.Services;

public sealed class GoalService : IGoalService
{
    private readonly ISessionService _sessionService;

    public GoalService(ISessionService sessionService)
    {
        _sessionService = sessionService;
    }

    public async Task<double> GetDailyGoalProgressAsync(CancellationToken cancellationToken = default)
    {
        var workedMinutes = await _sessionService.GetTodayWorkedMinutesAsync(cancellationToken);
        var goalMinutes = await _sessionService.GetDailyGoalMinutesAsync(cancellationToken);
        return Math.Clamp(workedMinutes / goalMinutes, 0, 1);
    }

    public Task<int> GetDailyStreakAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(4);
    }
}
