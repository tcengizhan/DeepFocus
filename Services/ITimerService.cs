namespace DeepFocus.Services;

public interface ITimerService
{
    event EventHandler? Tick;

    bool IsRunning { get; }

    void Start();

    void Stop();

    void Reset();
}
