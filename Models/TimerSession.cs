using System.Globalization;

namespace DeepFocus.Models;

public sealed class TimerSession
{
    private static readonly CultureInfo TrCulture = CultureInfo.GetCultureInfo("tr-TR");

    public int Id { get; set; }

    public DateTime StartedAt { get; set; }

    public DateTime? EndedAt { get; set; }

    public TimeSpan Duration { get; set; }

    public string Mode { get; set; } = string.Empty;

    public bool Completed { get; set; }

    public string TimerHistoryText => $"{Math.Round(Duration.TotalMinutes)} dk - {StartedAt.ToString("dd MMM HH:mm", TrCulture)}";

    public string DurationText => Duration.TotalHours >= 1
        ? Duration.ToString(@"hh\:mm\:ss")
        : Duration.ToString(@"mm\:ss");
}
