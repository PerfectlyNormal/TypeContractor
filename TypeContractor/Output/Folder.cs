namespace TypeContractor.Output;

public record Folder(string Name, string Path)
{
	public static Folder FromParts(string[] parts) => new(string.Join('.', parts), System.IO.Path.Combine(parts));
}