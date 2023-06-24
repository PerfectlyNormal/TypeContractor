namespace TypeContractor.Tool;

internal static class Log
{
    public static void LogError(string message)
    {
        Console.Error.WriteLine($"[ERR] {message}");
    }

#pragma warning disable IDE0060 // Remove unused parameter
    public static void LogError(Exception e, string message)
#pragma warning restore IDE0060 // Remove unused parameter
    {
        LogError(message);
    }

    public static void LogWarning(string message)
    {
        Console.WriteLine($"[WARN] {message}");
    }

    public static void LogMessage(string message)
    {
        Console.WriteLine($"[INFO] {message}");
    }
}
