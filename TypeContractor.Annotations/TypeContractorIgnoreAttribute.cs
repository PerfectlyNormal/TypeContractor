using System;

namespace TypeContractor.Annotations
{
	/// <summary>
	/// Tells TypeContractor to ignore the target when generating TypeScript.
	/// For example a controller that serves static assets for HTML templates.
	/// Can also be used for properties that don't need to be exposed.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	public sealed class TypeContractorIgnoreAttribute : Attribute
	{
	}
}
