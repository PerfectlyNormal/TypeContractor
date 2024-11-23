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

	[TypeContractorIgnore]
	internal class IgnoredController : ControllerBase { }
}
