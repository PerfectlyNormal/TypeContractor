using System;

namespace TypeContractor.Annotations
{
	/// <summary>
	/// Set a property as nullable if the compiler doesn't autodetect it.
	/// 
	/// <para>
	/// For example a string if your project is targetting netstandard2.0
	/// or another target framework that doesn't support nullable reference
	/// types, or nullable reference types is not enabled.
	/// </para>
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
	public sealed class TypeContractorNullableAttribute : Attribute
	{
	}
}
