namespace TypeContractor;

public enum LineEndings
{
	/// <summary>
	/// Windows line endings (\r\n)
	/// </summary>
	CrLf,

	/// <summary>
	/// Unix line endings (\n)
	/// </summary>
	Lf,
}

public static class LineEndingsExtensions
{
	public static string ToReplacementString(this LineEndings endings)
		=> endings switch
		{
			LineEndings.CrLf => "\r\n",
			LineEndings.Lf => "\n",
			_ => Environment.NewLine,
		};
}
