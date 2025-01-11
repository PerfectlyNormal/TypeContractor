namespace TypeContractor.Output;

public record DestinationType(
	string TypeName,
	string? FullName,
	string ImportType,
	bool IsBuiltin,
	bool IsArray,
	bool IsReadonly,
	bool IsNullable,
	bool IsGeneric,
	ICollection<DestinationType> GenericTypeArguments,
	Type? SourceType,
	Type? InnerType)
{
	public DestinationType(string typeName,
						string? fullName,
						bool isBuiltin,
						bool isArray,
						bool isReadonly,
						bool isNullable,
						bool isGeneric,
						ICollection<DestinationType> genericTypeArguments,
						Type? innerType,
						Type? sourceType,
						string? importType = null) : this(typeName, fullName, importType ?? typeName, isBuiltin, isArray, isReadonly, isNullable, isGeneric, genericTypeArguments, sourceType, innerType)
	{
	}

	/// <summary>
	/// Returns the <see cref="TypeName"/> and array brackets if the type is an array
	/// </summary>
	public string FullTypeName => $"{TypeName}{(IsArray ? "[]" : "")}";
}
