using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
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
        var assembly = typeof(TypeScriptWriterTests).Assembly;
        _outputDirectory = Directory.CreateTempSubdirectory();
        _configuration = TypeContractorConfiguration
            .WithDefaultConfiguration()
            .AddAssembly(assembly.FullName!, assembly.Location)
            .SetOutputDirectory(_outputDirectory.FullName);
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

    [Fact]
    public void Includes_Deprecated_JSDoc()
    {
        // Arrange
        var types = new[] { typeof(ObsoleteResponse) }
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
            .And.NotContain("import ")
            .And.Contain("export interface ObsoleteResponse {")
            .And.MatchRegex(@"/\*\*\r?\n\s+\* @deprecated\r?\n\s+\*/\r?\n\s+ obsoleteNoDesc: number;")
            .And.MatchRegex(@"/\*\*\r?\n\s+\* @deprecated Use NonObsoleteProp instead\r?\n\s+\*/\r?\n\s+ obsolete: number;")
            .And.MatchRegex(@"\s+nonObsoleteProp: number;");
    }

    [Fact]
    public void Includes_Deprecated_JSDoc_For_Enum_Members()
    {
        // Arrange
        var types = new[] { typeof(ObsoleteEnum) }
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
            .And.NotContain("import ")
            .And.Contain("export enum ObsoleteEnum {")
            .And.Contain("None = 0,")
            .And.Contain("Pending = 1,")
            .And.MatchRegex(@"/\*\*\r?\n\s+\* @deprecated No longer used\r?\n\s+\*/\r?\n\s+ Paid = 2,")
            .And.MatchRegex(@"/\*\*\r?\n\s+\* @deprecated\r?\n\s+\*/\r?\n\s+ Rejected = 3,")
            .And.Contain("Done = 4,");
    }

    [Fact]
    public void Handles_Nested_Nullable_Records()
    {
        // Arrange
        var types = new[] { typeof(TopLevelRecord) }
            .Select(t => ContractedType.FromName(t.FullName!, t, _configuration));

        var outputTypes = types
                .Select(_converter.Convert)
                .ToList() // Needed so `converter.Convert` runs before we concat
                .Concat(_converter.CustomMappedTypes.Values)
                .ToList();

        // Act
        var topLevelResult = Sut.Write(outputTypes.First(), outputTypes);
        var secondStoryResult = Sut.Write(outputTypes.First(x => x.Name == "SecondStoryRecord"), outputTypes);
        var someOtherDeeplyNestedResult = Sut.Write(outputTypes.First(x => x.Name == "SomeOtherDeeplyNestedRecord"), outputTypes);

        // Assert
        var topLevelFile = File.ReadAllText(topLevelResult);
        topLevelFile.Should()
            .NotBeEmpty()
            .And.Contain("import { SecondStoryRecord } from \"./SecondStoryRecord\";")
            .And.Contain("export interface TopLevelRecord {")
            .And.Contain("  name: string;")
            .And.Contain("  secondStoryRecord?: SecondStoryRecord;")
            .And.Contain("}");

        var secondStoryFile = File.ReadAllText(secondStoryResult);
        secondStoryFile.Should()
            .NotBeEmpty()
            .And.Contain("import { SomeOtherDeeplyNestedRecord } from \"./SomeOtherDeeplyNestedRecord\";")
            .And.Contain("export interface SecondStoryRecord {")
            .And.Contain("  description: string;")
            .And.Contain("  someOtherDeeplyNestedRecord?: SomeOtherDeeplyNestedRecord;")
            .And.Contain("}");

        var deeplyNestedFile = File.ReadAllText(someOtherDeeplyNestedResult);
        deeplyNestedFile.Should()
            .NotBeEmpty()
            .And.NotContain("import {")
            .And.Contain("export interface SomeOtherDeeplyNestedRecord {")
            .And.Contain("  extra: string;")
            .And.Contain("}");
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

    private class ObsoleteResponse
    {
        [Obsolete]
        public int ObsoleteNoDesc { get; set; }

        [Obsolete("Use NonObsoleteProp instead")]
        public double Obsolete { get; set; }

        public decimal NonObsoleteProp { get; set; }
    }

    private enum ObsoleteEnum
    {
        None,
        Pending,
        [Obsolete("No longer used")]
        Paid,
        [Obsolete]
        Rejected,
        Done,
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

    private record TopLevelRecord(string Name, SecondStoryRecord? SecondStoryRecord);
    private record SecondStoryRecord(string Description, SomeOtherDeeplyNestedRecord? SomeOtherDeeplyNestedRecord);
    private record SomeOtherDeeplyNestedRecord(string Extra);
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
