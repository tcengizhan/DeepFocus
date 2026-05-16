namespace DeepFocus.Models;

public sealed class TimerSession
{
    public int Id { get; set; }

    public DateTime StartedAt { get; set; }

    public DateTime? EndedAt { get; set; }

    public TimeSpan Duration { get; set; }

    public string Mode { get; set; } = string.Empty;

    public bool Completed { get; set; }
}
