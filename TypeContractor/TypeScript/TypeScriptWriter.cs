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

    public string Write(OutputType outputType, IEnumerable<OutputType> allTypes)
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

        // Return the path we wrote to
        return filePath;
    }

    private void BuildImports(OutputType type, IEnumerable<OutputType> allTypes)
    {
        var properties = type.Properties ?? Enumerable.Empty<OutputProperty>();
        var imports = properties
            .Where(p => !p.IsBuiltin)
            .DistinctBy(p => p.InnerSourceType ?? p.SourceType)
            .ToList();

        var alreadyImportedTypes = new List<string>();

        foreach (var import in imports)
        {
            var importedTypes = GetImportedTypes(allTypes, import);
            if (!importedTypes.Any())
                throw new ArgumentException($"Unable to find type for {import.SourceType}");

            foreach (var importedType in importedTypes)
            {
                if (alreadyImportedTypes.Contains(import.ImportType))
                    continue;

                try
                {
                    var relativePath = PathHelpers.RelativePath(importedType.ContractedType.Folder.Name, type.ContractedType.Folder.Name);
                    var importPath = $"{relativePath}/{importedType.Name}".Replace("//", "/", StringComparison.InvariantCultureIgnoreCase);

                    alreadyImportedTypes.Add(import.ImportType);
                    _builder.AppendLine(CultureInfo.InvariantCulture, $"import {{ {import.ImportType} }} from \"{importPath}\";");
                }
                catch (ArgumentException ex)
                {
                    throw new TypeScriptImportException(type, importedType, ex);
                }
            }
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

    private static List<OutputType> GetImportedTypes(IEnumerable<OutputType> allTypes, OutputProperty import)
    {
        var sourceType = import.SourceType;
        if (TypeChecks.ImplementsIEnumerable(import.SourceType) || TypeChecks.IsNullable(import.SourceType))
            sourceType = TypeChecks.GetGenericType(sourceType);

        if (TypeChecks.ImplementsIDictionary(import.SourceType))
        {
            var keyType = TypeChecks.GetGenericType(sourceType, 0);
            var valueType = TypeChecks.GetGenericType(sourceType, 1);

            return allTypes.Where(x => x.FullName == keyType.FullName || x.FullName == valueType.FullName).ToList();
        }

        return allTypes.Where(x => x.FullName == sourceType.FullName).ToList();
    }
}
