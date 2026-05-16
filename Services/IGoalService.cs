namespace DeepFocus.Services;

public interface IGoalService
{
    double GetDailyGoalProgress();

    int GetDailyStreak();
}
