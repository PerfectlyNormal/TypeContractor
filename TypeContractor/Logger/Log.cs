namespace TypeContractor.Logger;

public static class Log
{
	public static ILog Instance { get; set; } = new NullLogger();
}
