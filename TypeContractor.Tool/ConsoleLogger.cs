using TypeContractor.Logger;

namespace TypeContractor.Tool;

public enum LogLevel
{
	Debug,
	Info,
	Warning,
	Error,
}

internal class ConsoleLogger(LogLevel logLevel) : ILog
{
	public void LogError(string message)
	{
		if (logLevel <= LogLevel.Error)
			Console.Error.WriteLine($"[ ERR] {message}");
	}

	public void LogError(Exception exception, string message)
	{
		LogError(message);
		if (logLevel <= LogLevel.Debug)
			Console.Error.Write(exception);
	}

	public void LogWarning(string message)
	{
		if (logLevel <= LogLevel.Warning)
			Console.WriteLine($"[WARN] {message}");
	}

	public void LogMessage(string message)
	{
		if (logLevel <= LogLevel.Info)
			Console.WriteLine($"[INFO] {message}");
	}

	public void LogDebug(string message)
	{
		if (logLevel <= LogLevel.Debug)
			Console.WriteLine($"[ DBG] {message}");
	}
}
