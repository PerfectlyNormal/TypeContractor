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
