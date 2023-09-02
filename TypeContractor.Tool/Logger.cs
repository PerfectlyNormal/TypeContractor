using TypeContractor.Logger;

namespace TypeContractor.Tool;

public enum LogLevel
{
    Debug,
    Info,
    Warning,
    Error,
}

internal class Logger : ILog
{
    internal LogLevel _logLevel;

    public Logger(LogLevel level)
    {
        _logLevel = level;
    }

    public void LogError(string message)
    {
        if (_logLevel <= LogLevel.Error)
            Console.Error.WriteLine($"[ ERR] {message}");
    }

    public void LogError(Exception exception, string message)
    {
        LogError(message);
        if (_logLevel <= LogLevel.Debug)
            Console.Error.Write(exception);
    }

    public void LogWarning(string message)
    {
        if (_logLevel <= LogLevel.Warning)
            Console.WriteLine($"[WARN] {message}");
    }

    public void LogMessage(string message)
    {
        if (_logLevel <= LogLevel.Info)
            Console.WriteLine($"[INFO] {message}");
    }

    public void LogDebug(string message)
    {
        if (_logLevel <= LogLevel.Debug)
            Console.WriteLine($"[ DBG] {message}");
    }

    internal void SetLevel(LogLevel logLevel)
    {
        _logLevel = logLevel;
    }
}
