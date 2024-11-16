using System.Data;
using System.Text;
using TypeContractor.Helpers;
using TypeContractor.Output;

namespace TypeContractor.TypeScript;

#pragma warning disable CA1305 // Specify IFormatProvider
public class TypeScriptWriter(string outputPath)
{
	private static readonly Encoding _utf8WithoutBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
	private readonly StringBuilder _builder = new();

	public string Write(OutputType outputType, IEnumerable<OutputType> allTypes, bool buildZodSchema)
	{
		ArgumentNullException.ThrowIfNull(outputType);

		_builder.Clear();

		BuildImports(outputType, allTypes, buildZodSchema);
		BuildExport(outputType);
		if (buildZodSchema)
			ZodSchemaWriter.Write(outputType, allTypes, _builder);

		var directory = Path.Combine(outputPath, outputType.ContractedType.Folder.Path);
		var filePath = Path.Combine(directory, $"{outputType.Name}.ts");

		// Create directory if needed
		if (!Directory.Exists(directory))
			Directory.CreateDirectory(directory);

		// Write file
		File.WriteAllText(filePath, _builder.ToString().Trim() + Environment.NewLine + Environment.NewLine, _utf8WithoutBom);

		// Return the path we wrote to
		return filePath;
	}

	private void BuildImports(OutputType type, IEnumerable<OutputType> allTypes, bool buildZodSchema)
	{
		var properties = type.Properties ?? Enumerable.Empty<OutputProperty>();
		var imports = properties
			.Where(p => !p.IsBuiltin)
			.DistinctBy(p => p.InnerSourceType ?? p.SourceType)
			.ToList();

		if (buildZodSchema)
			_builder.AppendLine(ZodSchemaWriter.LibraryImport);

		var alreadyImportedTypes = new List<string>();

		foreach (var import in imports)
		{
			var importedTypes = GetImportedTypes(allTypes, import);
			if (importedTypes.Count == 0)
			{
				var references = allTypes
					.Where(x => x.Properties is not null)
					.Select(x =>
					{
						var properties = x.Properties!.Where(prop => prop.SourceType.FullName == type.FullName || prop.InnerSourceType?.FullName == type.FullName);
						return new TypeScriptReference(x, properties);
					})
					.Where(x => x.Properties.Any());

				throw new TypeScriptReferenceException(type, import, references, new ArgumentException($"Unable to find type for {import.SourceType}"));
			}

			foreach (var importedType in importedTypes)
			{
				if (alreadyImportedTypes.Contains(import.ImportType))
					continue;

				try
				{
					var relativePath = PathHelpers.RelativePath(importedType.ContractedType.Folder.Name, type.ContractedType.Folder.Name);
					var importPath = $"{relativePath}/{importedType.Name}".Replace("//", "/", StringComparison.InvariantCultureIgnoreCase);

					alreadyImportedTypes.Add(import.ImportType);
					var importTypes = new List<string> { import.ImportType };
					if (buildZodSchema)
					{
						var zodImport = ZodSchemaWriter.BuildImport(import);
						if (!string.IsNullOrWhiteSpace(zodImport))
							importTypes.Add(zodImport);
					}

					_builder.AppendLine($"import {{ {string.Join(", ", importTypes)} }} from '{importPath}';");
				}
				catch (ArgumentException ex)
				{
					throw new TypeScriptImportException(type, importedType, ex);
				}
			}
		}

		if (imports.Count > 0 || buildZodSchema)
			_builder.AppendLine();
	}

	private void BuildExport(OutputType type)
	{
		// Header
		if (type.IsEnum)
		{
			_builder.AppendLine($"export enum {type.Name} {{");
		}
		else
		{
			_builder.AppendLine($"export interface {type.Name} {{");
		}

		// Body
		foreach (var property in type.Properties ?? Enumerable.Empty<OutputProperty>())
		{
			var nullable = property.IsNullable ? "?" : "";
			var array = property.IsArray ? "[]" : "";
			var isReadonly = property.IsReadonly ? "readonly " : "";

			_builder.AppendDeprecationComment(property.Obsolete);
			_builder.AppendFormat("  {4}{0}{1}: {2}{3};\r\n", property.DestinationName, nullable, property.DestinationType, array, isReadonly);
		}

		foreach (var member in type.EnumMembers ?? Enumerable.Empty<OutputEnumMember>())
		{
			_builder.AppendDeprecationComment(member.Obsolete);
			_builder.AppendFormat("  {0} = {1},\r\n", member.DestinationName, member.DestinationValue);
		}

		// Footer
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
			while (TypeChecks.ImplementsIEnumerable(valueType))
				valueType = TypeChecks.GetGenericType(valueType);

			if (TypeChecks.ImplementsIDictionary(valueType))
			{
				var nestedKeyType = TypeChecks.GetGenericType(valueType, 0);
				var nestedValueType = TypeChecks.GetGenericType(valueType, 1);
				while (TypeChecks.ImplementsIEnumerable(nestedValueType))
					nestedValueType = TypeChecks.GetGenericType(nestedValueType);

				var names = new[] { keyType.FullName, nestedKeyType.FullName, nestedValueType.FullName };
				return allTypes.Where(x => names.Contains(x.FullName)).ToList();
			}

			return allTypes.Where(x => x.FullName == keyType.FullName || x.FullName == valueType.FullName).ToList();
		}

		return allTypes.Where(x => x.FullName == sourceType.FullName).ToList();
	}
}
#pragma warning restore CA1305 // Specify IFormatProvider
