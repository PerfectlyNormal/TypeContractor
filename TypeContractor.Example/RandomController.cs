using Microsoft.AspNetCore.Mvc;
using TypeContractor.Annotations;

namespace TypeContractor.Example;

[TypeContractorClient("RandomizerClient")]
public class RandomController : ControllerBase
{
	private readonly Random _random = new();

	public ActionResult<int> GenerateRandomValue(CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		return _random.Next(0, 256);
	}
}
