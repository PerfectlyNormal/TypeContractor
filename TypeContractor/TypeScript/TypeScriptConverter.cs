using System.Reflection;
using TypeContractor.Helpers;
using TypeContractor.Output;

namespace TypeContractor.TypeScript;

public class TypeScriptConverter
{
    private readonly TypeContractorConfiguration _configuration;
    private readonly MetadataLoadContext _metadataLoadContext;

    public TypeScriptConverter(TypeContractorConfiguration configuration, MetadataLoadContext metadataLoadContext)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _metadataLoadContext = metadataLoadContext ?? throw new ArgumentNullException(nameof(metadataLoadContext));
    }

    public Dictionary<Type, OutputType> CustomMappedTypes { get; } = new();

    public OutputType Convert(ContractedType contractedType)
    {
        if (contractedType is null) throw new ArgumentNullException(nameof(contractedType));
        return Convert(contractedType.Type, contractedType);
    }

    public OutputType Convert(Type type, ContractedType? contractedType = null)
    {
        if (type is null) throw new ArgumentNullException(nameof(type));

        return new(
            type.Name,
            type.FullName!,
            contractedType ?? ContractedType.FromName(type.FullName!, type, _configuration),
            type.IsEnum,
            type.IsEnum ? null : GetProperties(type).Distinct().ToList(),
            type.IsEnum ? GetEnumProperties(type) : null
        );
    }

    private ICollection<OutputEnumMember> GetEnumProperties(Type type)
    {
        var matchAssembly = _metadataLoadContext.LoadFromAssemblyName(type.Assembly.FullName!);
        var matchedEnumType = matchAssembly.GetType(type.FullName!)!;

        var underlyingValues = matchedEnumType.GetEnumValuesAsUnderlyingType();

        return matchedEnumType
            .GetEnumNames()
            .Select((name, idx) => new OutputEnumMember(name, name, underlyingValues.GetValue(idx)!))
            .ToList();
    }

    private ICollection<OutputProperty> GetProperties(Type type)
    {
        var outputProperties = new List<OutputProperty>();

        // Find all properties
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        // Evaluate type of property
        foreach (var property in properties)
        {
            // Need to have a getter
            if (!property.CanRead) continue;

            // Getter has to be public
            var getter = property.GetGetMethod(false);
            if (getter is null) continue;

            var destinationName = GetDestinationName(property.Name);
            var destinationType = GetDestinationType(property.PropertyType);
            outputProperties.Add(new OutputProperty(property.Name, property.PropertyType, destinationType.InnerType, destinationName, destinationType.TypeName, destinationType.ImportType, destinationType.IsBuiltin, destinationType.IsArray, TypeChecks.IsNullable(property.PropertyType)));
        }

        // Look at base classes
        if (type.BaseType is not null)
            outputProperties.AddRange(GetProperties(type.BaseType));

        // Look at interfaces
        foreach (var iface in type.GetInterfaces())
            outputProperties.AddRange(GetProperties(iface));

        return outputProperties;
    }

    private static string GetDestinationName(string name) => name.ToTypeScriptName();

    private DestinationType GetDestinationType(in Type sourceType)
    {
        if (_configuration.TypeMaps.TryGetValue(sourceType.FullName!, out string? destType))
            return new DestinationType(destType, true, false, null);

        if (CustomMappedTypes.TryGetValue(sourceType, out OutputType? customType))
            return new DestinationType(customType.Name, false, false, null);

        if (TypeChecks.ImplementsIDictionary(sourceType))
        {
            var keyType = GetDestinationType(TypeChecks.GetGenericType(sourceType, 0));
            var valueType = TypeChecks.GetGenericType(sourceType, 1);
            var valueDestinationType = GetDestinationType(valueType);

            var isBuiltin = keyType.IsBuiltin && valueDestinationType.IsBuiltin;

            return new DestinationType($"{{ [key: {keyType.TypeName}]: {valueDestinationType.TypeName} }}", isBuiltin, false, valueType, valueDestinationType.TypeName);
        }

        if (TypeChecks.ImplementsIEnumerable(sourceType))
        {
            var innerType = TypeChecks.GetGenericType(sourceType);

            var (TypeName, _, IsBuiltin, _, _) = GetDestinationType(innerType);
            return new DestinationType(TypeName, IsBuiltin, true, innerType);
        }

        if (TypeChecks.IsNullable(sourceType))
        {
            return GetDestinationType(sourceType.GenericTypeArguments.First());
        }

        // FIXME: Check if this is one of our types?
        var outputType = Convert(sourceType);
        CustomMappedTypes.Add(sourceType, outputType);
        return new DestinationType(outputType.Name, false, false, null);

        // throw new ArgumentException($"Unexpected type: {sourceType}");
    }
}
