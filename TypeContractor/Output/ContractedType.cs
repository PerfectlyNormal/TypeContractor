namespace TypeContractor.Output;

public record ContractedType(string Name, string FullName, Type Type, Folder Folder)
{
    public static ContractedType FromName(string name, Type type, TypeContractorConfiguration configuration)
    {
        if (configuration is null) throw new ArgumentNullException(nameof(configuration));

        var nameparts = ApplyReplacements(name, configuration.Replacements).Split('.');
        var typeName = nameparts.Last();
        var folderName = nameparts.Take(..^1).ToList();

        if (type.IsNestedPublic && type.DeclaringType is not null)
            folderName.Add(type.DeclaringType.Name);

        return new ContractedType(typeName, name, type, Folder.FromParts(folderName.ToArray()));
    }

    public static string ApplyReplacements(string input, IReadOnlyDictionary<string, string> replacements)
    {
        if (input is null) throw new ArgumentNullException(nameof(input));
        if (replacements is null) throw new ArgumentNullException(nameof(replacements));

        foreach (var (needle, replacement) in replacements)
            input = input.Replace(needle, replacement, StringComparison.InvariantCulture);

        return input;
    }
}
