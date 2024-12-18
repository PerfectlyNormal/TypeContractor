using Microsoft.AspNetCore.Mvc;
using TypeContractor.Annotations;

namespace TypeContractor.Example;

[TypeContractorName("RandomizerClient")]
public class RandomController : ControllerBase
{
	private readonly Random _random = new();

	[TypeContractorName("randomize")]
	public ActionResult<int> GenerateRandomValue(CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		return _random.Next(0, 256);
	}
}
