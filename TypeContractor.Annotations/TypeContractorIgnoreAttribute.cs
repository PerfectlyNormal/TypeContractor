using System;

namespace TypeContractor.Annotations
{
	/// <summary>
	/// Tells TypeContractor to ignore the target when generating TypeScript.
	/// For example a controller that serves static assets for HTML templates.
	/// Can also be used for properties that don't need to be exposed.
	/// <para>
	/// When generating API clients, a single endpoint can be ignored using
	/// this attribute.
	/// </para>
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	public sealed class TypeContractorIgnoreAttribute : Attribute
	{
	}
}
