using DeepFocus.Models;
using Microsoft.Data.Sqlite;
using System.IO;

namespace DeepFocus.Services;

public sealed class SessionService : ISessionService
{
    private readonly string _connectionString;

    public SessionService()
    {
        var databasePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "DeepFocus",
            "deepfocus.db");

        Directory.CreateDirectory(Path.GetDirectoryName(databasePath)!);
        _connectionString = new SqliteConnectionStringBuilder { DataSource = databasePath }.ToString();
        InitializeDatabase();
    }

    public event EventHandler? SessionsChanged;

    public Task<IReadOnlyList<TimerSession>> GetRecentSessionsAsync(CancellationToken cancellationToken = default)
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, StartedAt, EndedAt, DurationSeconds, Mode, Completed
            FROM TimerSessions
            ORDER BY StartedAt DESC
            LIMIT 25;
            """;

        using var reader = command.ExecuteReader();
        var sessions = new List<TimerSession>();

        while (reader.Read())
        {
            sessions.Add(new TimerSession
            {
                Id = reader.GetInt32(0),
                StartedAt = DateTime.Parse(reader.GetString(1)),
                EndedAt = reader.IsDBNull(2) ? null : DateTime.Parse(reader.GetString(2)),
                Duration = TimeSpan.FromSeconds(reader.GetDouble(3)),
                Mode = reader.GetString(4),
                Completed = reader.GetInt32(5) == 1
            });
        }

        return Task.FromResult<IReadOnlyList<TimerSession>>(sessions);
    }

    public Task AddSessionAsync(TimerSession session, CancellationToken cancellationToken = default)
    {
        if (session.Duration <= TimeSpan.Zero)
        {
            return Task.CompletedTask;
        }

        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO TimerSessions (StartedAt, EndedAt, DurationSeconds, Mode, Completed)
            VALUES ($startedAt, $endedAt, $durationSeconds, $mode, $completed);
            """;
        command.Parameters.AddWithValue("$startedAt", session.StartedAt.ToString("O"));
        command.Parameters.AddWithValue("$endedAt", session.EndedAt is null ? DBNull.Value : session.EndedAt.Value.ToString("O"));
        command.Parameters.AddWithValue("$durationSeconds", session.Duration.TotalSeconds);
        command.Parameters.AddWithValue("$mode", session.Mode);
        command.Parameters.AddWithValue("$completed", session.Completed ? 1 : 0);
        command.ExecuteNonQuery();

        SessionsChanged?.Invoke(this, EventArgs.Empty);
        return Task.CompletedTask;
    }

    public Task<double[]> GetCurrentWeekMinutesAsync(CancellationToken cancellationToken = default)
    {
        var weekStart = GetCurrentWeekStart();
        var weekEnd = weekStart.AddDays(7);
        var minutes = new double[7];

        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT StartedAt, DurationSeconds
            FROM TimerSessions
            WHERE StartedAt >= $weekStart AND StartedAt < $weekEnd;
            """;
        command.Parameters.AddWithValue("$weekStart", weekStart.ToString("O"));
        command.Parameters.AddWithValue("$weekEnd", weekEnd.ToString("O"));

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var startedAt = DateTime.Parse(reader.GetString(0));
            var index = (int)(startedAt.Date - weekStart).TotalDays;
            if (index is >= 0 and < 7)
            {
                minutes[index] += TimeSpan.FromSeconds(reader.GetDouble(1)).TotalMinutes;
            }
        }

        return Task.FromResult(minutes);
    }

    public Task<double> GetTodayWorkedMinutesAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);

        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT COALESCE(SUM(DurationSeconds), 0)
            FROM TimerSessions
            WHERE StartedAt >= $today AND StartedAt < $tomorrow;
            """;
        command.Parameters.AddWithValue("$today", today.ToString("O"));
        command.Parameters.AddWithValue("$tomorrow", tomorrow.ToString("O"));

        return Task.FromResult(TimeSpan.FromSeconds(Convert.ToDouble(command.ExecuteScalar())).TotalMinutes);
    }

    public Task<int> GetDailyGoalMinutesAsync(CancellationToken cancellationToken = default)
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Value FROM Preferences WHERE Key = 'DailyGoalMinutes';";
        var value = command.ExecuteScalar()?.ToString();
        return Task.FromResult(int.TryParse(value, out var minutes) ? minutes : 120);
    }

    public Task SetDailyGoalMinutesAsync(int minutes, CancellationToken cancellationToken = default)
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO Preferences (Key, Value)
            VALUES ('DailyGoalMinutes', $value)
            ON CONFLICT(Key) DO UPDATE SET Value = excluded.Value;
            """;
        command.Parameters.AddWithValue("$value", Math.Max(1, minutes).ToString());
        command.ExecuteNonQuery();

        SessionsChanged?.Invoke(this, EventArgs.Empty);
        return Task.CompletedTask;
    }

    private void InitializeDatabase()
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS TimerSessions (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                StartedAt TEXT NOT NULL,
                EndedAt TEXT NULL,
                DurationSeconds REAL NOT NULL,
                Mode TEXT NOT NULL,
                Completed INTEGER NOT NULL
            );

            CREATE TABLE IF NOT EXISTS Preferences (
                Key TEXT PRIMARY KEY,
                Value TEXT NOT NULL
            );

            INSERT OR IGNORE INTO Preferences (Key, Value)
            VALUES ('DailyGoalMinutes', '120');
            """;
        command.ExecuteNonQuery();
    }

    private SqliteConnection OpenConnection()
    {
        var connection = new SqliteConnection(_connectionString);
        connection.Open();
        return connection;
    }

    private static DateTime GetCurrentWeekStart()
    {
        var today = DateTime.Today;
        var daysSinceMonday = ((int)today.DayOfWeek + 6) % 7;
        return today.AddDays(-daysSinceMonday);
    }
}
