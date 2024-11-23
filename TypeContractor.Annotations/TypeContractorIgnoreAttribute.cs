using System;

namespace TypeContractor.Annotations
{
	/// <summary>
	/// Tells TypeContractor to ignore the given controller when generating
	/// automatic API clients. For example a controller that serves static
	/// assets for the HTML templates.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public sealed class TypeContractorIgnoreAttribute : Attribute
	{
	}
}
