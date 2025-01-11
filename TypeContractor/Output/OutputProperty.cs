namespace TypeContractor.Output;

public class OutputProperty(
	string sourceName,
	Type sourceType,
	Type? innerSourceType,
	string destinationName,
	string destinationType,
	string importType,
	bool isBuiltin,
	bool isArray,
	bool isNullable,
	bool isReadonly,
	bool isGeneric,
	ICollection<DestinationType> genericTypeArguments)
{
	public string SourceName { get; set; } = sourceName;
	public Type SourceType { get; set; } = sourceType;
	public Type? InnerSourceType { get; set; } = innerSourceType;
	public string DestinationName { get; set; } = destinationName;
	public string DestinationType { get; set; } = destinationType;
	public string ImportType { get; set; } = importType;
	public bool IsBuiltin { get; set; } = isBuiltin;
	public bool IsArray { get; set; } = isArray;
	public bool IsNullable { get; set; } = isNullable;
	public bool IsReadonly { get; set; } = isReadonly;
	public bool IsGeneric { get; set; } = isGeneric;
	public ICollection<DestinationType> GenericTypeArguments { get; } = genericTypeArguments;
	public ObsoleteInfo? Obsolete { get; set; }

	/// <summary>
	/// Returns the <see cref="DestinationType"/> and array brackets if the type is an array
	/// </summary>
	public string FullDestinationType => $"{DestinationType}{(IsArray ? "[]" : "")}";

	public override string ToString()
	{
		return $"{(IsReadonly ? "readonly" : "")}{DestinationName}{(IsNullable ? "?" : "")}: {FullDestinationType} (import {ImportType} from {SourceType}, {(IsBuiltin ? "builtin" : "custom")})";
	}

	public override bool Equals(object? obj)
	{
		return obj is OutputProperty property &&
			   SourceName == property.SourceName &&
			   EqualityComparer<Type>.Default.Equals(SourceType, property.SourceType) &&
			   EqualityComparer<Type?>.Default.Equals(InnerSourceType, property.InnerSourceType) &&
			   DestinationName == property.DestinationName &&
			   DestinationType == property.DestinationType &&
			   ImportType == property.ImportType &&
			   IsBuiltin == property.IsBuiltin &&
			   IsArray == property.IsArray &&
			   IsNullable == property.IsNullable &&
			   IsReadonly == property.IsReadonly &&
			   IsGeneric == property.IsGeneric &&
			   GenericTypeArguments.SequenceEqual(property.GenericTypeArguments) &&
			   EqualityComparer<ObsoleteInfo?>.Default.Equals(Obsolete, property.Obsolete);
	}

	public override int GetHashCode()
	{
		var hash = new HashCode();
		hash.Add(SourceName);
		hash.Add(SourceType);
		hash.Add(InnerSourceType);
		hash.Add(DestinationName);
		hash.Add(DestinationType);
		hash.Add(ImportType);
		hash.Add(IsBuiltin);
		hash.Add(IsArray);
		hash.Add(IsNullable);
		hash.Add(IsReadonly);
		hash.Add(IsGeneric);
		hash.Add(Obsolete);
		return hash.ToHashCode();
	}

	public static bool operator ==(OutputProperty? left, OutputProperty? right)
	{
		return EqualityComparer<OutputProperty>.Default.Equals(left, right);
	}

	public static bool operator !=(OutputProperty? left, OutputProperty? right)
	{
		return !(left == right);
	}
}
