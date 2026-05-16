using DeepFocus.Models;

namespace DeepFocus.Services;

public interface ISessionService
{
    Task<IReadOnlyList<TimerSession>> GetRecentSessionsAsync(CancellationToken cancellationToken = default);

    Task AddSessionAsync(TimerSession session, CancellationToken cancellationToken = default);
}
