using Microsoft.AspNetCore.Mvc;
using System.Collections;
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
                .GetField(fieldName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?? throw new ArgumentNullException(nameof(fieldName), $"Unable to find field '{fieldName}'");

            TypeChecks.IsNullable(field.FieldType).Should().BeTrue();
        }

        [Theory]
        [InlineData(nameof(_nullableString))]
        [InlineData(nameof(_customCollection))]
        [InlineData(nameof(_enum))]
        public void IsNullable_Is_False_For_Reference_Types(string fieldName)
        {
            var field = typeof(TypeChecksTests)
                .GetField(fieldName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?? throw new ArgumentNullException(nameof(fieldName), $"Unable to find field '{fieldName}'");

            TypeChecks.IsNullable(field.FieldType).Should().BeFalse();
        }

        [Theory]
        [InlineData(typeof(List<string>))]
        [InlineData(typeof(IEnumerable<string>))]
        [InlineData(typeof(string[]))]
        [InlineData(typeof(byte[]))]
        [InlineData(typeof(CustomListWrapper))]
        [InlineData(typeof(ComplexListWrapper))]
        [InlineData(typeof(ICollection<string>))]
        [InlineData(typeof(IReadOnlyCollection<string>))]
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
        [InlineData(typeof(Dictionary<string, int>))]
        [InlineData(typeof(Dictionary<int, int>))]
        [InlineData(typeof(Dictionary<int, string>))]
        [InlineData(typeof(IDictionary<string, int>))]
        [InlineData(typeof(IDictionary<int, int>))]
        [InlineData(typeof(IDictionary<int, string>))]
        [InlineData(typeof(IReadOnlyDictionary<string, int>))]
        [InlineData(typeof(IReadOnlyDictionary<int, int>))]
        [InlineData(typeof(IReadOnlyDictionary<int, string>))]
        [InlineData(typeof(ICollection))]
        [InlineData(typeof(IDictionary))]
        public void ImplementsIEnumerable_Is_False_For_Invalid_Targets(Type target)
        {
            TypeChecks.ImplementsIEnumerable(target).Should().BeFalse();
        }

        [Theory]
        [InlineData(typeof(Dictionary<string, int>))]
        [InlineData(typeof(Dictionary<int, int>))]
        [InlineData(typeof(Dictionary<int, string>))]
        [InlineData(typeof(IDictionary<string, int>))]
        [InlineData(typeof(IDictionary<int, int>))]
        [InlineData(typeof(IDictionary<int, string>))]
        [InlineData(typeof(IReadOnlyDictionary<string, int>))]
        [InlineData(typeof(IReadOnlyDictionary<int, int>))]
        [InlineData(typeof(IReadOnlyDictionary<int, string>))]
        public void ImplementsDictionary_Is_True_For_Targets(Type target)
        {
            TypeChecks.ImplementsIDictionary(target).Should().BeTrue();
        }

        [Theory]
        [InlineData(typeof((string, int)))]
        [InlineData(typeof(ValueTuple<string>))]
        [InlineData(typeof(ValueTuple<string, int, int>))]
        public void IsValueTuple_Is_True_For_Targets(Type target)
        {
            TypeChecks.IsValueTuple(target).Should().BeTrue();
        }

        [Theory]
        [InlineData(typeof(string))]
        [InlineData(typeof(ValueTuple))]
        [InlineData(typeof(Dictionary<int, (int, string)>))]
        public void IsValueTuple_Is_False_For_Invalid_Targets(Type target)
        {
            TypeChecks.IsValueTuple(target).Should().BeFalse();
        }

        [Theory]
        [InlineData(typeof(string))]
        [InlineData(typeof(IDictionary<string, int>))]
        [InlineData(typeof(CustomListWrapper))]
        public void IsController_Is_False_For_Invalid_Targets(Type target)
        {
            TypeChecks.IsController(target).Should().BeFalse();
        }

        [Theory]
        [InlineData(typeof(ControllerBase))]
        [InlineData(typeof(Controller))]
        [InlineData(typeof(CustomApplicationController))]
        [InlineData(typeof(NestedController))]
        public void IsController_Is_True_For_Valid_Targets(Type target)
        {
            TypeChecks.IsController(target).Should().BeTrue();
        }

        [Theory]
        [InlineData(true, nameof(NestedController.GetNumberAsync))]
        [InlineData(true, nameof(NestedController.GetNumber))]
        [InlineData(false, nameof(NestedController.LegacyEndpointAsync))]
        [InlineData(false, nameof(NestedController.LegacyEndpoint))]
        [InlineData(false, nameof(NestedController.WhatEvenIsThisAsync))]
        public void ReturnsActionResult_Returns_Correct_Value(bool expectedResult, string methodName)
        {
            var target = typeof(NestedController).GetMethod(methodName);
            if (target is null)
                Assert.Fail("Unable to find method " + methodName + " on controller");
            
            TypeChecks.ReturnsActionResult(target).Should().Be(expectedResult);
        }

        [Theory]
        [InlineData(typeof(CustomCollection), nameof(ReturnTypeController.GetCustomCollection))]
        [InlineData(typeof(ComplexNestedType), nameof(ReturnTypeController.GetListOfObjects))]
        public void UnwrappedReturnType_Returns_Correct_Type(Type? expectedType, string methodName)
        {
            var target = typeof(ReturnTypeController).GetMethod(methodName);
            if (target is null)
                Assert.Fail("Unable to find method " + methodName + " on controller");

            var returnType = TypeChecks.UnwrappedReturnType(target);

            if (expectedType is null)
                returnType.Should().BeNull();
            else
                returnType.Should().Be(expectedType);
        }

        [Theory]
        [InlineData(null, nameof(ReturnTypeController.GetRandomGuid))]
        [InlineData(null, nameof(ReturnTypeController.SomeStringMethod))]
        [InlineData(null, nameof(ReturnTypeController.ListOfNumbers))]
        public void UnwrappedReturnType_Ignores_Builtins(Type? expectedType, string methodName)
        {
            var target = typeof(ReturnTypeController).GetMethod(methodName);
            if (target is null)
                Assert.Fail("Unable to find method " + methodName + " on controller");

            var returnType = TypeChecks.UnwrappedReturnType(target);

            if (expectedType is null)
                returnType.Should().BeNull();
            else
                returnType.Should().Be(expectedType);
        }
    }


    internal class CustomApplicationController : ControllerBase
    { }

    internal class NestedController : CustomApplicationController
    {
        public Task<ActionResult<int>> GetNumberAsync() => throw new NotImplementedException();
        public ActionResult<int> GetNumber() => throw new NotImplementedException();
        public Task<IActionResult> LegacyEndpointAsync() => throw new NotImplementedException();
        public IActionResult LegacyEndpoint() => throw new NotImplementedException();
        public Task<string> WhatEvenIsThisAsync() => throw new NotImplementedException();
    }

    internal class ReturnTypeController : CustomApplicationController
    {
        public Task<ActionResult<Guid>> GetRandomGuid() => throw new NotImplementedException();
        public ActionResult<string> SomeStringMethod() => throw new NotImplementedException();
        public ActionResult<CustomCollection> GetCustomCollection() => throw new NotImplementedException();
        public ActionResult<IEnumerable<int>> ListOfNumbers() => throw new NotImplementedException();
        public ActionResult<List<ComplexNestedType>> GetListOfObjects() => throw new NotImplementedException();
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

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    internal class ComplexNestedType
    {
        public int ErrorCode { get; set; }
        public Guid ErrorScopeId { get; set; }
        public string Message { get; set; }
        public string Reference { get; set; } = "";
        public Guid SequentialId { get; set; }
        public string SourceHint { get; set; }
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}
