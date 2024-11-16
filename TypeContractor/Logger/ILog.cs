namespace TypeContractor.Logger;

public interface ILog
{
	void LogDebug(string message);
	void LogError(Exception exception, string message);
	void LogError(string message);
	void LogMessage(string message);
	void LogWarning(string message);
}
