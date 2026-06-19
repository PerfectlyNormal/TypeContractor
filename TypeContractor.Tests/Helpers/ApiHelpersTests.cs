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

	[Fact]
	public void BuildApiEndpoint_Skips_Ignored_Methods()
	{
		// Arrange
		var endpointMethod = typeof(LegacyController).GetMethod(nameof(LegacyController.NothingToSeeHere), [typeof(CancellationToken)])!;

		// Act
		var endpoint = ApiHelpers.BuildApiEndpoint(endpointMethod);

		// Assert
		endpoint.Should().BeEmpty();
	}

	[Fact]
	public void BuildApiEndpoint_Handles_Optional_Route_Part()
	{
		// Arrange
		var endpointMethod = typeof(RouteController).GetMethod(nameof(RouteController.DeleteWithOptionalPart), [typeof(Guid), typeof(Guid), typeof(Guid), typeof(Guid), typeof(Guid?), typeof(CancellationToken)])!;

		// Act
		var endpoints = ApiHelpers.BuildApiEndpoint(endpointMethod);

		// Assert
		endpoints.Should().ContainSingle();
		var endpoint = endpoints.First();

		endpoint.Route.Should().Be("deletemember/{id}/{referenceId}/{certificationGroupId?}");
		endpoint.Parameters.Should()
			.HaveCount(3)
			.And.Contain(x => x.FromRoute && x.Name == "id" && !x.IsOptional)
			.And.Contain(x => x.FromRoute && x.Name == "referenceId" && !x.IsOptional)
			.And.Contain(x => x.FromRoute && x.Name == "certificationGroupId" && x.IsOptional);
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

		[HttpPatch("third-route")]
		[TypeContractorIgnore]
		public ActionResult NothingToSeeHere(CancellationToken cancellationToken) => NotFound();
	}

	[TypeContractorName("RenamedApi")]
	internal class RenamedSuffixController : ControllerBase { }

	internal class RouteController : ControllerBase
	{
		[HttpDelete("deletemember/{id:Guid}/{referenceId:Guid}/{certificationGroupId:Guid?}")]
		public ActionResult DeleteWithOptionalPart([FromHeader] Guid organizationId, [FromHeader] Guid customerId, Guid id, Guid referenceId, Guid? certificationGroupId, CancellationToken cancellationToken) => NotFound();
	}
}
