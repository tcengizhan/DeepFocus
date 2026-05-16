namespace DeepFocus.Models;

public sealed class LapRecord
{
    public int Number { get; set; }

    public TimeSpan Elapsed { get; set; }

    public DateTime RecordedAt { get; set; } = DateTime.Now;

    public string Label => $"TUR {Number}";

    public string DisplayTime => Elapsed.ToString(@"mm\:ss\.ff");
}
