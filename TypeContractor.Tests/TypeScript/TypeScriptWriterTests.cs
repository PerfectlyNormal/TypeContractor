using System.Reflection;
using System.Runtime.InteropServices;
using TypeContractor.Output;
using TypeContractor.TypeScript;

namespace TypeContractor.Tests.TypeScript;

public class TypeScriptWriterTests : IDisposable
{
    private readonly DirectoryInfo _outputDirectory;
    private readonly TypeContractorConfiguration _configuration;
    private readonly TypeScriptConverter _converter;

    public TypeScriptWriter Sut { get; }

    public TypeScriptWriterTests()
    {
        _outputDirectory = Directory.CreateTempSubdirectory();
        _configuration = TypeContractorConfiguration.WithDefaultConfiguration().SetOutputDirectory(_outputDirectory.FullName);
        _converter = new TypeScriptConverter(_configuration, BuildMetadataLoadContext());
        Sut = new TypeScriptWriter(_configuration.OutputPath);
    }

    [Fact]
    public void Can_Write_Simple_Types()
    {
        // Arrange
        var types = new[] { typeof(SimpleTypes) }
            .Select(t => ContractedType.FromName(t.FullName!, t, _configuration));

        var outputTypes = types
                .Select(_converter.Convert)
                .ToList() // Needed so `converter.Convert` runs before we concat
                .Concat(_converter.CustomMappedTypes.Values)
                .ToList();

        // Act
        var result = Sut.Write(outputTypes.First(), outputTypes);

        // Assert
        var file = File.ReadAllLines(result).Select(x => x.TrimStart());
        file.Should()
            .NotBeEmpty()
            .And.NotContainMatch("import * from")
            .And.Contain("export interface SimpleTypes {")
            .And.Contain("stringProperty: string;")
            .And.Contain("numberProperty?: number;")
            .And.Contain("numbersProperty: number[];")
            .And.Contain("doubleTime: number;")
            .And.Contain("timeyWimeySpan: string;")
            .And.Contain("someObject: any;");
    }

    [Fact]
    public void Handles_Dictionary_With_Complex_Values()
    {
        // Arrange
        var types = new[] { typeof(ComplexValueDictionary) }
            .Select(t => ContractedType.FromName(t.FullName!, t, _configuration));

        var outputTypes = types
                .Select(_converter.Convert)
                .ToList() // Needed so `converter.Convert` runs before we concat
                .Concat(_converter.CustomMappedTypes.Values)
                .ToList();

        // Act
        var result = Sut.Write(outputTypes.First(), outputTypes);

        // Assert
        var file = File.ReadAllText(result);
        file.Should()
            .NotBeEmpty()
            .And.Contain("import { FormulaDto } from \"./FormulaDto\";")
            .And.Contain("formulas: { [key: string]: FormulaDto[] };");
    }

    [Fact]
    public void Handles_Dictionary_With_Nested_Dictionary_Values()
    {
        // Arrange
        var types = new[] { typeof(NestedValueDictionary) }
            .Select(t => ContractedType.FromName(t.FullName!, t, _configuration));

        var outputTypes = types
                .Select(_converter.Convert)
                .ToList() // Needed so `converter.Convert` runs before we concat
                .Concat(_converter.CustomMappedTypes.Values)
                .ToList();

        // Act
        var result = Sut.Write(outputTypes.First(), outputTypes);

        // Assert
        var file = File.ReadAllText(result);
        file.Should()
            .NotBeEmpty()
            .And.Contain("import { FormulaDto } from \"./FormulaDto\";")
            .And.Contain("formulas: { [key: string]: { [key: string]: FormulaDto[] } };");
    }

    #region Test input
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private class SimpleTypes
    {
        public string StringProperty { get; set; }
        public int? NumberProperty { get; set; }
        public IEnumerable<int> NumbersProperty { get; set; }
        public double DoubleTime { get; set; }
        public TimeSpan TimeyWimeySpan { get; set; }
        public object SomeObject { get; set; }
    }

    private class ComplexValueDictionary
    {
        public Dictionary<Guid, IEnumerable<FormulaDto>> Formulas { get; set; }
    }

    private class NestedValueDictionary
    {
        public Dictionary<Guid, Dictionary<string, IEnumerable<FormulaDto>>> Formulas { get; set; }
    }

    private class FormulaDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Definition { get; set; }
    }
    #endregion
#pragma warning restore CS8618

    private MetadataLoadContext BuildMetadataLoadContext()
    {
        // Get the array of runtime assemblies.
        var runtimeAssemblies = Directory.GetFiles(RuntimeEnvironment.GetRuntimeDirectory(), "*.dll");

        // Create the list of assembly paths consisting of runtime assemblies and the inspected assemblies.
        var paths = runtimeAssemblies.Concat(_configuration.Assemblies.Values);

        var resolver = new PathAssemblyResolver(paths);

        return new MetadataLoadContext(resolver);
    }

    public void Dispose()
    {
        if (_outputDirectory.Exists)
            _outputDirectory.Delete(true);
    }
}
