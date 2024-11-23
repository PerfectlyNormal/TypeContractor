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

	[TypeContractorIgnore]
	internal class IgnoredController : ControllerBase { }

	[TypeContractorClient("RenamedClient")]
	internal class LegacyController : ControllerBase { }

	[TypeContractorClient("RenamedApi")]
	internal class RenamedSuffixController : ControllerBase { }
}
