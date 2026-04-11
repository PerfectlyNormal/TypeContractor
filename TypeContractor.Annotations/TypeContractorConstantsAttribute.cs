using System;

namespace TypeContractor.Annotations
{
	/// <summary>
	/// Tells TypeContractor to find all constant/static strings and generate
	/// a TypeScript equivalent class with the same constants.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public sealed class TypeContractorConstantsAttribute : Attribute
	{
	}
}
