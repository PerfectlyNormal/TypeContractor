using Newtonsoft.Json;
using TypeContractor.Helpers;

namespace TypeContractor.Tests.Helpers
{
    public class TypeChecksTests
    {
#pragma warning disable CS0649
        public string? _nullableString;
        public decimal? _nullableDecimal;
        public bool? _nullableBoolean;
        internal CustomCollection? _customCollection;
        internal MyEnum _enum;
        internal MyEnum? _nullableEnum;
#pragma warning restore CS0649

        [Theory]
        [InlineData(nameof(_nullableDecimal))]
        [InlineData(nameof(_nullableBoolean))]
        [InlineData(nameof(_nullableEnum))]
        public void IsNullable_Is_True_For_Nullable_Types(string fieldName)
        {
            var field = typeof(TypeChecksTests)
                .GetField(fieldName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (field is null)
                throw new ArgumentNullException(nameof(fieldName), $"Unable to find field '{fieldName}'");

            TypeChecks.IsNullable(field.FieldType).Should().BeTrue();
        }

        [Theory]
        [InlineData(nameof(_nullableString))]
        [InlineData(nameof(_customCollection))]
        [InlineData(nameof(_enum))]
        public void IsNullable_Is_False_For_Reference_Types(string fieldName)
        {
            var field = typeof(TypeChecksTests)
                .GetField(fieldName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (field is null)
                throw new ArgumentNullException(nameof(fieldName), $"Unable to find field '{fieldName}'");

            TypeChecks.IsNullable(field.FieldType).Should().BeFalse();
        }

        [Theory]
        [InlineData(typeof(List<string>))]
        [InlineData(typeof(IEnumerable<string>))]
        [InlineData(typeof(string[]))]
        [InlineData(typeof(byte[]))]
        [InlineData(typeof(CustomListWrapper))]
        [InlineData(typeof(ComplexListWrapper))]
        public void ImplementsIEnumerable_Is_True_For_Targets(Type target)
        {
            TypeChecks.ImplementsIEnumerable(target).Should().BeTrue();
        }

        [Theory]
        [InlineData(typeof(string))]
        [InlineData(typeof(int))]
        [InlineData(typeof(MyEnum))]
        [InlineData(typeof(byte))]
        [InlineData(typeof(CustomCollection))]
        public void ImplementsIEnumerable_Is_False_For_Invalid_Targets(Type target)
        {
            TypeChecks.ImplementsIEnumerable(target).Should().BeFalse();
        }
    }

    internal class CustomListWrapper : List<string>
    { }

    internal enum MyEnum
    {
        None,
        Error,
    }

    internal class CustomCollection
    {
        public IEnumerable<string>? Items { get; set; }
    }

    internal class ComplexListWrapper : List<ComplexNestedType>
    { }

    internal class ComplexNestedType
    {
        public int ErrorCode { get; set; }
        public Guid ErrorScopeId { get; set; }
        public string Message { get; set; }
        public string Reference { get; set; } = "";
        public Guid SequentialId { get; set; }
        public string SourceHint { get; set; }
    }
}
