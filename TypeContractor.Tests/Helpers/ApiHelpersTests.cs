using Microsoft.AspNetCore.Mvc;
using TypeContractor.Annotations;
using TypeContractor.Helpers;

namespace TypeContractor.Tests.Helpers;

public class ApiHelpersTests
{
	[Fact]
	public void BuildApiClient_Returns_Null_Given_IgnoreAttribute()
	{
		var client = ApiHelpers.BuildApiClient(typeof(IgnoredController), []);

		client.Should().BeNull();
	}

	[Fact]
	public void BuildApiClient_Accepts_ClientAttribute()
	{
		var client = ApiHelpers.BuildApiClient(typeof(LegacyController), []);

		client.Should().NotBeNull();
		client!.Name.Should().Be("RenamedClient");
	}

	[Fact]
	public void BuildApiClient_Does_Not_Add_Suffix_With_ClientAttribute()
	{
		var client = ApiHelpers.BuildApiClient(typeof(RenamedSuffixController), []);

		client.Should().NotBeNull();
		client!.Name.Should().Be("RenamedApi");
	}

	[Fact]
	public void BuildApiEndpoint_Accepts_NameAttribute()
	{
		// Arrange
		var endpointMethod = typeof(LegacyController).GetMethod(nameof(LegacyController.OverloadEndpoint), [typeof(Guid), typeof(CancellationToken)])!;

		// Act
		var endpoint = ApiHelpers.BuildApiEndpoint(endpointMethod);

		// Assert
		endpoint.Should().ContainSingle();
		endpoint.First().Name.Should().Be("postWithId");
	}

	[Fact]
	public void BuildApiEndpoint_Generates_Name()
	{
		// Arrange
		var endpointMethod = typeof(LegacyController).GetMethod(nameof(LegacyController.OverloadEndpoint), [typeof(CancellationToken)])!;

		// Act
		var endpoint = ApiHelpers.BuildApiEndpoint(endpointMethod);

		// Assert
		endpoint.Should().ContainSingle();
		endpoint.First().Name.Should().Be("overloadEndpoint");
	}

	[TypeContractorIgnore]
	internal class IgnoredController : ControllerBase { }

	[TypeContractorName("RenamedClient")]
	internal class LegacyController : ControllerBase
	{
		[HttpPost("many-methods")]
		[TypeContractorName("postWithId")]
		public ActionResult OverloadEndpoint(Guid id, CancellationToken cancellationToken) => NotFound();

		[HttpGet("other-route")]
		public ActionResult OverloadEndpoint(CancellationToken cancellationToken) => NotFound();
	}

	[TypeContractorName("RenamedApi")]
	internal class RenamedSuffixController : ControllerBase { }
}
