using System.Windows.Threading;

namespace DeepFocus.Services;

public sealed class TimerService : ITimerService
{
    private readonly DispatcherTimer _timer;

    public TimerService()
    {
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _timer.Tick += (_, _) => Tick?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler? Tick;

    public bool IsRunning => _timer.IsEnabled;

    public void Start()
    {
        _timer.Start();
    }

    public void Stop()
    {
        _timer.Stop();
    }

    public void Reset()
    {
        _timer.Stop();
    }
}
