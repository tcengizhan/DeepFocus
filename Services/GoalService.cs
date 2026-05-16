using System.Text.Json;
using System.IO;

namespace DeepFocus.Services;

public sealed class GoalService : IGoalService
{
    private readonly string _preferencesPath;

    public GoalService()
    {
        _preferencesPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "DeepFocus",
            "preferences.json");
    }

    public double GetDailyGoalProgress()
    {
        _ = JsonSerializer.Serialize(new { DailyGoalMinutes = 120 });
        return 0.42;
    }

    public int GetDailyStreak()
    {
        _ = _preferencesPath;
        return 4;
    }
}
