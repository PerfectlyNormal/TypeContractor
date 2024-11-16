namespace TypeContractor.Logger;

public class NullLogger : ILog
{
	public void LogDebug(string message)
	{
	}

	public void LogError(Exception exception, string message)
	{
	}

	public void LogError(string message)
	{
	}

	public void LogMessage(string message)
	{
	}

	public void LogWarning(string message)
	{
	}
}
