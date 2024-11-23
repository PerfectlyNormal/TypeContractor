using Microsoft.AspNetCore.Mvc;
using TypeContractor.Annotations;

namespace TypeContractor.Example;

[TypeContractorIgnore]
public class StaticAssetsController : ControllerBase
{
	public ActionResult GetSomeAsset()
	{
		return NotFound();
	}
}
