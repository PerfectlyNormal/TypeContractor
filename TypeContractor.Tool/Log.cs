namespace TypeContractor.Tool;

internal static class Log
{
    public static void LogError(string message)
    {
        Console.Error.WriteLine($"[ERR] {message}");
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
