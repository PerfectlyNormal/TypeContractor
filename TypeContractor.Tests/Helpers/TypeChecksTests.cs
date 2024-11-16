using Microsoft.AspNetCore.Mvc;
using System.Collections;
using System.Reflection;
using System.Reflection.Metadata;
using TypeContractor.Helpers;

namespace TypeContractor.Tests.Helpers
{
	public class TypeChecksTests
	{
#pragma warning disable CS0649
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
		public string _regularString = "";
		public string? _nullableString;
		public decimal? _nullableDecimal;
		public bool? _nullableBoolean;
		internal CustomCollection? _customCollection;
		internal MyEnum _enum;
		internal MyEnum? _nullableEnum;
		internal MyRecord? _nullableRecord;
		internal MyRecord _regularRecordReference;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
#pragma warning restore CS0649

		[Theory]
		[InlineData(nameof(_nullableDecimal))]
		[InlineData(nameof(_nullableBoolean))]
		[InlineData(nameof(_nullableEnum))]
		[InlineData(nameof(_nullableRecord))]
		[InlineData(nameof(_nullableString))]
		[InlineData(nameof(_customCollection))]
		public void IsNullable_Is_True_For_Nullable_Types(string fieldName)
		{
			var field = typeof(TypeChecksTests)
				.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
				?? throw new ArgumentNullException(nameof(fieldName), $"Unable to find field '{fieldName}'");

			TypeChecks.IsNullable(field).Should().BeTrue();
		}

		[Theory]
		[InlineData(nameof(_regularString))]
		[InlineData(nameof(_enum))]
		[InlineData(nameof(_regularRecordReference))]
		public void IsNullable_Is_False_For_Reference_Types(string fieldName)
		{
			var field = typeof(TypeChecksTests)
				.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
				?? throw new ArgumentNullException(nameof(fieldName), $"Unable to find field '{fieldName}'");

			TypeChecks.IsNullable(field).Should().BeFalse();
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
		[InlineData(true, nameof(NestedController.FileStreamEndpoint))]
		[InlineData(true, nameof(NestedController.FileStreamEndpointAsync))]
		[InlineData(true, nameof(NestedController.LegacyEndpointAsync))]
		[InlineData(true, nameof(NestedController.LegacyEndpoint))]
		[InlineData(false, nameof(NestedController.WhatEvenIsThisAsync))]
		public void ReturnsActionResult_Returns_Correct_Value(bool expectedResult, string methodName)
		{
			var target = typeof(NestedController).GetMethod(methodName);
			if (target is null)
				Assert.Fail("Unable to find method " + methodName + " on controller");

			TypeChecks.ReturnsActionResult(target).Should().Be(expectedResult);
		}

		[Fact]
		public void ReturnsActionResult_Ignores_NonAction_Methods()
		{
			var methods = typeof(ControllerBase).GetMethods();

			foreach (var method in methods)
			{
				var hasNonActionAttribute = method.GetCustomAttribute<NonActionAttribute>() != null;
				var result = TypeChecks.ReturnsActionResult(method);
				if (hasNonActionAttribute && result)
					Assert.Fail($"Expected method {method.Name} to not return ActionResult, but it does.");
			}
		}

		[Theory]
		[InlineData(typeof(CustomCollection), nameof(ReturnTypeController.GetCustomCollection))]
		[InlineData(typeof(ComplexNestedType), nameof(ReturnTypeController.GetListOfObjects))]
		public void UnwrappedReturnType_Returns_Correct_Type(Type? expectedType, string methodName)
		{
			var target = typeof(ReturnTypeController).GetMethod(methodName);
			if (target is null)
				Assert.Fail("Unable to find method " + methodName + " on controller");

			var returnType = TypeChecks.FullyUnwrappedReturnType(target);

			returnType.Should().Be(expectedType);
		}

		[Theory]
		[InlineData(nameof(ReturnTypeController.GetRandomGuid))]
		[InlineData(nameof(ReturnTypeController.SomeStringMethod))]
		[InlineData(nameof(ReturnTypeController.ListOfNumbers))]
		[InlineData(nameof(ReturnTypeController.FileStreamEndpoint))]
		[InlineData(nameof(ReturnTypeController.FileStreamEndpointAsync))]
		public void UnwrappedReturnType_Ignores_Builtins(string methodName)
		{
			var target = typeof(ReturnTypeController).GetMethod(methodName);
			if (target is null)
				Assert.Fail("Unable to find method " + methodName + " on controller");

			var returnType = TypeChecks.FullyUnwrappedReturnType(target);

			returnType.Should().BeNull();
		}

		[Theory]
		[InlineData(nameof(ParameterTypeController.CreateObject), new[] { typeof(ComplexRequest) })]
		[InlineData(nameof(ParameterTypeController.CreateManyObjects), new[] { typeof(ComplexRequest) })]
		[InlineData(nameof(ParameterTypeController.MultipleDtos), new[] { typeof(ComplexRequest), typeof(SimpleRequest) })]
		public void UnwrappedParameters_Returns_Correct_Type(string methodName, Type[] expectedTypes)
		{
			var target = typeof(ParameterTypeController).GetMethod(methodName);
			if (target is null)
				Assert.Fail("Unable to find method " + methodName + " on controller");

			var parameterTypes = TypeChecks.UnwrappedParameters(target);

			if (expectedTypes is null)
				parameterTypes.Should().BeEmpty();
			else
				parameterTypes.Should().ContainInOrder(expectedTypes);
		}

		[Theory]
		[InlineData(nameof(ParameterTypeController.GetById), null)]
		[InlineData(nameof(ParameterTypeController.GetByNumericId), null)]
		[InlineData(nameof(ParameterTypeController.GetWithListOfIds), null)]
		public void UnwrappedParameters_Ignores_Builtins(string methodName, Type[]? expectedTypes)
		{
			var target = typeof(ParameterTypeController).GetMethod(methodName);
			if (target is null)
				Assert.Fail("Unable to find method " + methodName + " on controller");

			var parameterTypes = TypeChecks.UnwrappedParameters(target);

			if (expectedTypes is null)
				parameterTypes.Should().BeEmpty();
			else
				parameterTypes.Should().ContainInOrder(expectedTypes);
		}

		[Theory]
		[InlineData(nameof(ParameterTypeController.GetWithExtraServices), null)]
		public void UnwrappedParameters_Ignores_AspNetFramework_Types(string methodName, Type[]? expectedTypes)
		{
			var target = typeof(ParameterTypeController).GetMethod(methodName);
			if (target is null)
				Assert.Fail("Unable to find method " + methodName + " on controller");

			var parameterTypes = TypeChecks.UnwrappedParameters(target);

			if (expectedTypes is null)
				parameterTypes.Should().BeEmpty();
			else
				parameterTypes.Should().ContainInOrder(expectedTypes);
		}
	}


	internal class CustomApplicationController : ControllerBase
	{ }

	internal class NestedController : CustomApplicationController
	{
		public Task<ActionResult<int>> GetNumberAsync() => throw new NotImplementedException();
		public ActionResult<int> GetNumber() => throw new NotImplementedException();
		public Task<IActionResult> LegacyEndpointAsync() => throw new NotImplementedException();
		public ActionResult FileStreamEndpoint() => throw new NotImplementedException();
		public Task<ActionResult> FileStreamEndpointAsync() => throw new NotImplementedException();
		public IActionResult LegacyEndpoint() => throw new NotImplementedException();
		public Task<string> WhatEvenIsThisAsync() => throw new NotImplementedException();
	}

	internal class ReturnTypeController : CustomApplicationController
	{
		public Task<ActionResult<Guid>> GetRandomGuid() => throw new NotImplementedException();
		public ActionResult<string> SomeStringMethod() => throw new NotImplementedException();
		public ActionResult FileStreamEndpoint() => throw new NotImplementedException();
		public Task<ActionResult> FileStreamEndpointAsync() => throw new NotImplementedException();
		public ActionResult<CustomCollection> GetCustomCollection() => throw new NotImplementedException();
		public ActionResult<IEnumerable<int>> ListOfNumbers() => throw new NotImplementedException();
		public ActionResult<List<ComplexNestedType>> GetListOfObjects() => throw new NotImplementedException();
	}

	internal class ParameterTypeController : ControllerBase
	{
		public Task<ActionResult<ComplexNestedType>> GetById(Guid id, CancellationToken cancellationToken) => throw new NotImplementedException();
		public Task<ActionResult<ComplexNestedType>> GetByNumericId(int id, [FromQuery] string search, CancellationToken cancellationToken) => throw new NotImplementedException();
		public Task<ActionResult<IEnumerable<ComplexNestedType>>> GetWithListOfIds([FromBody] IEnumerable<Guid> organizationId, CancellationToken cancellationToken) => throw new NotImplementedException();
		public Task<ActionResult<IEnumerable<ComplexNestedType>>> GetWithExtraServices([FromBody] IEnumerable<Guid> organizationId, [FromServices] SimpleRequest requestConfiguration, CancellationToken cancellationToken) => throw new NotImplementedException();
		public Task<ActionResult<Guid>> CreateObject(ComplexRequest request, CancellationToken cancellationToken) => throw new NotImplementedException();
		public Task<ActionResult<IEnumerable<Guid>>> CreateManyObjects(IEnumerable<ComplexRequest> requests, CancellationToken cancellationToken) => throw new NotImplementedException();
		public Task<ActionResult<IEnumerable<Guid>>> MultipleDtos(ComplexRequest request, SimpleRequest extraData, CancellationToken cancellationToken) => throw new NotImplementedException();
	}

	internal class CustomListWrapper : List<string>
	{ }

	internal enum MyEnum
	{
		None,
		Error,
	}

	internal record MyRecord(string Name);

	internal class CustomCollection
	{
		public IEnumerable<string>? Items { get; set; }
	}

	internal class ComplexListWrapper : List<ComplexNestedType>
	{ }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
	internal class ComplexRequest
	{
		public Guid OrganizationId { get; set; }
		public string Name { get; set; }
		public int Count { get; set; }
	}

	internal class SimpleRequest
	{
		public string CustomerName { get; set; }
	}

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
