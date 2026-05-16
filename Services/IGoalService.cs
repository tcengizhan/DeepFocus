namespace DeepFocus.Services;

public interface IGoalService
{
    Task<double> GetDailyGoalProgressAsync(CancellationToken cancellationToken = default);

    Task<int> GetDailyStreakAsync(CancellationToken cancellationToken = default);
}
