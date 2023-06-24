namespace TypeContractor.Tool;

public enum LogLevel
{
    Debug,
    Info,
    Warning,
    Error,
}

internal static class Log
{
    internal static LogLevel _logLevel;

    public static void LogError(string message)
    {
        if (_logLevel <= LogLevel.Error)
            Console.Error.WriteLine($"[ ERR] {message}");
    }

#pragma warning disable IDE0060 // Remove unused parameter
    public static void LogError(Exception e, string message)
#pragma warning restore IDE0060 // Remove unused parameter
    {
        LogError(message);
    }

    public static void LogWarning(string message)
    {
        if (_logLevel <= LogLevel.Warning)
            Console.WriteLine($"[WARN] {message}");
    }

    public static void LogMessage(string message)
    {
        if (_logLevel <= LogLevel.Info)
            Console.WriteLine($"[INFO] {message}");
    }

    public static void LogDebug(string message)
    {
        if (_logLevel <= LogLevel.Debug)
            Console.WriteLine($"[ DBG] {message}");
    }

    internal static void SetLevel(LogLevel logLevel)
    {
        _logLevel = logLevel;
    }
}
