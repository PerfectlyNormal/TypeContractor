using TypeContractor.Output;

namespace TypeContractor.TypeScript;

internal class TypeScriptReferenceException : Exception
{
	public OutputType? Type { get; }
	public OutputProperty? PropertyWithError { get; }
	public IEnumerable<TypeScriptReference> References { get; }

	public TypeScriptReferenceException()
	{
		References = [];
	}

	public TypeScriptReferenceException(string? message) : base(message)
	{
		References = [];
	}

	public TypeScriptReferenceException(OutputType type, OutputProperty propertyWithError, IEnumerable<TypeScriptReference> references, Exception innerException)
		: this($"Unable to find referenced type for property {propertyWithError.SourceName} in {type.Name}", innerException)
	{
		Type = type;
		PropertyWithError = propertyWithError;
		References = references;
	}

	public TypeScriptReferenceException(string? message, Exception? innerException) : base(message, innerException)
	{
		References = [];
	}
}
