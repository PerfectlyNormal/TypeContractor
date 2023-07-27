using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using TypeContractor.TypeScript;

namespace TypeContractor.Tests.TypeScript;

public class TypeScriptConverterTests
{
    private readonly TypeContractorConfiguration _configuration;

    public TypeScriptConverter Sut { get; }

    public TypeScriptConverterTests()
    {
        _configuration = TypeContractorConfiguration.WithDefaultConfiguration();
        Sut = new TypeScriptConverter(_configuration, BuildMetadataLoadContext());
    }

    [Fact]
    public void Throws_Given_Invalid_Input()
    {
        Sut.Invoking(c => c.Convert(null!))
            .Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Can_Convert_Simple_Types()
    {
        var result = Sut.Convert(typeof(SimpleTypes));

        result.Should().NotBeNull();
        result.Name.Should().Be("SimpleTypes");
        result.FullName.Should().Be("TypeContractor.Tests.TypeScript.TypeScriptConverterTests+SimpleTypes");
        result.IsEnum.Should().BeFalse();
        result.EnumMembers.Should().BeNull();
        result.Properties.Should().HaveCount(3);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void Converted_Simple_Properties_Looks_As_Expected(int propertyIndex)
    {
        var result = Sut.Convert(typeof(SimpleTypes));
        var prop = result.Properties!.ElementAt(propertyIndex);

        prop.Should().NotBeNull();
        switch (propertyIndex)
        {
            case 0:
                prop.SourceName.Should().Be("StringProperty");
                prop.SourceType.Should().Be(typeof(string));
                prop.InnerSourceType.Should().BeNull();
                prop.DestinationName.Should().Be("stringProperty");
                prop.DestinationType.Should().Be("string");
                prop.IsBuiltin.Should().BeTrue();
                prop.IsArray.Should().BeFalse();
                prop.IsNullable.Should().BeFalse();
                break;

            case 1:
                prop.SourceName.Should().Be("NumberProperty");
                prop.SourceType.Should().Be(typeof(int?));
                prop.InnerSourceType.Should().BeNull();
                prop.DestinationName.Should().Be("numberProperty");
                prop.DestinationType.Should().Be("number");
                prop.IsBuiltin.Should().BeTrue();
                prop.IsArray.Should().BeFalse();
                prop.IsNullable.Should().BeTrue();
                break;

            case 2:
                prop.SourceName.Should().Be("NumbersProperty");
                prop.SourceType.Should().Be(typeof(IEnumerable<int>));
                prop.InnerSourceType.Should().Be(typeof(int));
                prop.DestinationName.Should().Be("numbersProperty");
                prop.DestinationType.Should().Be("number");
                prop.IsBuiltin.Should().BeTrue();
                prop.IsArray.Should().BeTrue();
                prop.IsNullable.Should().BeFalse();
                break;
        }
    }

    [Fact]
    public void Only_Converts_Visible_Publicly_Readable_Properties()
    {
        var result = Sut.Convert(typeof(TypeVisibility));

        result.Properties.Should().NotBeNull();
        result.Properties.Should().ContainSingle();
        result.Properties!.First().SourceName.Should().Be("ReadOnlyNumber");
    }

    [Fact]
    public void Finds_Properties_From_Multiple_Base_Classes()
    {
        var result = Sut.Convert(typeof(NestedInheritanceTest));

        result.Properties.Should().NotBeNull();
        result.Properties.Should().HaveCount(5);
        result.Properties.Should()
            .Contain(x => x.SourceName == "StringProperty")
            .And.Contain(x => x.SourceName == "NumberProperty")
            .And.Contain(x => x.SourceName == "NumbersProperty")
            .And.Contain(x => x.SourceName == "InheritedProperty")
            .And.Contain(x => x.SourceName == "FinalProperty");
    }

    [Fact]
    public void Nested_Classes_Gets_DeclaringType_As_Folder_Prefix()
    {
        var result = Sut.Convert(typeof(YearSummary.Response));

        result.Should().NotBeNull();
        result.Name.Should().Be("Response");
        result.ContractedType.Folder.Path.Should().EndWith("YearSummary");
    }

    [Fact]
    public void Handles_Dictionary_With_Tuple_Values()
    {
        var result = Sut.Convert(typeof(ComplexDictionaryResponse));

        result.Should().NotBeNull();
        result.Properties.Should().ContainSingle();
        var prop = result.Properties!.First();
        prop.DestinationName.Should().Be("migrationWarnings");
        prop.DestinationType.Should().Be("{ [key: string]: { item1: string[], item2: string[] } }");
    }

    [Fact]
    public void Handles_Simple_ValueTuple_Types()
    {
        var result = Sut.Convert(typeof(ValueTupleResponse));

        result.Should().NotBeNull();
        result.Properties.Should().ContainSingle();
        var prop = result.Properties!.First();
        prop.DestinationName.Should().Be("messages");
        prop.DestinationType.Should().Be("{ item1: string[], item2: string[] }");
    }

    [Fact]
    public void Handles_Dynamic_Types()
    {
        var result = Sut.Convert(typeof(DynamicResponse));

        result.Should().NotBeNull();
        result.Properties.Should().ContainSingle();
        var prop = result.Properties!.First();
        prop.DestinationName.Should().Be("result");
        prop.DestinationType.Should().Be("any");
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private class SimpleTypes
    {
        public string StringProperty { get; set; }
        public int? NumberProperty { get; set; }
        public IEnumerable<int> NumbersProperty { get; set; }
    }

    private class TypeVisibility
    {
#pragma warning disable IDE0051 // Remove unused private members
#pragma warning disable IDE1006 // Naming Styles
        private string _stringProp { get; set; }
#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore IDE0051 // Remove unused private members
        public int WriteOnlyNumber { private get; set; }
        public int ReadOnlyNumber { get; private set; }
    }

    private class InheritanceTest : SimpleTypes
    {
        public string InheritedProperty { get; set; }
    }

    private class NestedInheritanceTest : InheritanceTest
    {
        public string FinalProperty { get; set; }
    }

    private class YearSummary
    {
        public class Request
        {
            public Guid OrganizationId { get; set; }
        }

        public class Response
        {
            public IEnumerable<int> Years { get; set; }
            public Dictionary<int, int> PaymentsPerYear { get; set; }
        }
    }

    private class ComplexDictionaryResponse
    {
        public Dictionary<string, (List<string> Errors, List<string> Warnings)> MigrationWarnings { get; set; }
    }

    private class ValueTupleResponse
    {
        public (List<string> Errors, List<string> Warnings) Messages { get; set; }
    }

    private class DynamicResponse
    {
        public dynamic Result { get; set; }
    }

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    private MetadataLoadContext BuildMetadataLoadContext()
    {
        // Get the array of runtime assemblies.
        var runtimeAssemblies = Directory.GetFiles(RuntimeEnvironment.GetRuntimeDirectory(), "*.dll");

        // Create the list of assembly paths consisting of runtime assemblies and the inspected assemblies.
        var paths = runtimeAssemblies.Concat(_configuration.Assemblies.Values);

        var resolver = new PathAssemblyResolver(paths);

        return new MetadataLoadContext(resolver);
    }
}
