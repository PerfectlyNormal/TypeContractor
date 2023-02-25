using System.Globalization;
using System.Text;
using TypeContractor.Helpers;
using TypeContractor.Output;

namespace TypeContractor.TypeScript;

public class TypeScriptWriter
{
    private readonly StringBuilder _builder;
    private readonly string _outputPath;

    public TypeScriptWriter(string outputPath)
    {
        _builder = new StringBuilder();
        _outputPath = outputPath;
    }

    public void Write(OutputType outputType, IEnumerable<OutputType> allTypes)
    {
        if (outputType is null)
            throw new ArgumentNullException(nameof(outputType));

        _builder.Clear();

        BuildImports(outputType, allTypes);
        BuildHeader(outputType);
        BuildBody(outputType);
        BuildFooter();

        var directory = Path.Combine(_outputPath, outputType.ContractedType.Folder.Path);
        var filePath = Path.Combine(directory, $"{outputType.Name}.ts");

        // Create directory if needed
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // Write file
        File.WriteAllText(filePath, _builder.ToString());
    }

    private void BuildImports(OutputType type, IEnumerable<OutputType> allTypes)
    {
        var properties = type.Properties ?? Enumerable.Empty<OutputProperty>();
        var imports = properties
            .Where(p => !p.IsBuiltin)
            .DistinctBy(p => p.InnerSourceType ?? p.SourceType)
            .ToList();

        foreach (var import in imports)
        {
            var importedType = GetImportedType(allTypes, import);
            if (importedType is null)
                throw new ArgumentException($"Unable to find type for {import.SourceType}");

            var relativePath = PathHelpers.RelativePath(importedType.ContractedType.Folder.Name, type.ContractedType.Folder.Name);
            var importPath = $"{relativePath}/{importedType.Name}".Replace("//", "/", StringComparison.InvariantCultureIgnoreCase);

            _builder.AppendLine(CultureInfo.InvariantCulture, $"import {{ {import.DestinationType} }} from \"{importPath}\";");
        }

        if (imports.Any())
            _builder.AppendLine();
    }

    private void BuildHeader(OutputType type)
    {
        if (type.IsEnum)
        {
            _builder.AppendLine(CultureInfo.InvariantCulture, $"export enum {type.Name} {{");
        }
        else
        {
            _builder.AppendLine(CultureInfo.InvariantCulture, $"export interface {type.Name} {{");
        }
    }

    private void BuildBody(OutputType type)
    {
        foreach (var property in type.Properties ?? Enumerable.Empty<OutputProperty>())
        {
            var nullable = property.IsNullable ? "?" : "";
            var array = property.IsArray ? "[]" : "";
            _builder.AppendFormat(CultureInfo.InvariantCulture, "  {0}{1}: {2}{3};\r\n", property.DestinationName, nullable, property.DestinationType, array);
        }

        foreach (var member in type.EnumMembers ?? Enumerable.Empty<OutputEnumMember>())
        {
            _builder.AppendFormat(CultureInfo.InvariantCulture, "  {0} = {1},\r\n", member.DestinationName, member.DestinationValue);
        }
    }

    private void BuildFooter()
    {
        _builder.AppendLine("}");
    }

    private static OutputType? GetImportedType(IEnumerable<OutputType> allTypes, OutputProperty import)
    {
        var sourceType = import.SourceType;
        if (TypeChecks.ImplementsIEnumerable(import.SourceType) || TypeChecks.IsNullable(import.SourceType))
            sourceType = sourceType.GenericTypeArguments.First();

        return allTypes.FirstOrDefault(x => x.FullName == sourceType.FullName);
    }
}
