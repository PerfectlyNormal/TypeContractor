using TypeContractor.Output;

namespace TypeContractor.TypeScript;

internal class TypeScriptReference(OutputType outputType, IEnumerable<OutputProperty> properties)
{
	public OutputType Type { get; set; } = outputType;
	public IEnumerable<OutputProperty> Properties { get; set; } = properties;
}
