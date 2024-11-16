namespace TypeContractor.Output;

public record ContractedType(string Name, string FullName, Type Type, Folder Folder)
{
	public static ContractedType FromName(string name, Type type, TypeContractorConfiguration configuration)
	{
		ArgumentNullException.ThrowIfNull(type, nameof(type));
		ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));

		var nameparts = ApplyReplacements(name, configuration.Replacements).Split('.');
		var typeName = nameparts.Last();
		var folderName = nameparts.Take(..^1).ToList();

		if (type.IsNestedPublic && type.DeclaringType is not null)
			folderName.Add(type.DeclaringType.Name);

		return new ContractedType(typeName, name, type, Folder.FromParts([.. folderName]));
	}

	public static string ApplyReplacements(string input, IReadOnlyDictionary<string, string> replacements)
	{
		ArgumentNullException.ThrowIfNull(input);
		ArgumentNullException.ThrowIfNull(replacements);

		foreach (var (needle, replacement) in replacements)
			input = input.Replace(needle, replacement, StringComparison.InvariantCulture);

		return input;
	}
}
