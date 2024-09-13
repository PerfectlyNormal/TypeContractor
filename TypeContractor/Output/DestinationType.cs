namespace TypeContractor.Output;

public record DestinationType(string TypeName, string? FullName, string ImportType, bool IsBuiltin, bool IsArray, bool IsReadonly, Type? InnerType)
{
    public DestinationType(string typeName, string? fullName, bool isBuiltin, bool isArray, bool isReadonly, Type? innerType, string? importType = null) : this(typeName, fullName, importType ?? typeName, isBuiltin, isArray, isReadonly, innerType)
    {
    }

    /// <summary>
    /// Returns the <see cref="TypeName"/> and array brackets if the type is an array
    /// </summary>
    public string FullTypeName => $"{TypeName}{(IsArray ? "[]" : "")}";
}
