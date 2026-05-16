using DeepFocus.Models;
using Microsoft.Data.Sqlite;
using System.IO;

namespace DeepFocus.Services;

public sealed class SessionService : ISessionService
{
    private readonly string _connectionString;
    private readonly List<TimerSession> _sessions = [];

    public SessionService()
    {
        var databasePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "DeepFocus",
            "deepfocus.db");

        Directory.CreateDirectory(Path.GetDirectoryName(databasePath)!);
        _connectionString = new SqliteConnectionStringBuilder { DataSource = databasePath }.ToString();
    }

    public Task<IReadOnlyList<TimerSession>> GetRecentSessionsAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<TimerSession> sessions = _sessions
            .OrderByDescending(session => session.StartedAt)
            .Take(25)
            .ToList();

        return Task.FromResult(sessions);
    }

    public Task AddSessionAsync(TimerSession session, CancellationToken cancellationToken = default)
    {
        _sessions.Add(session);
        return Task.CompletedTask;
    }
}
