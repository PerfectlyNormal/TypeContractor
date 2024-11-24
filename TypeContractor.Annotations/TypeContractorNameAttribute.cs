using System;

namespace TypeContractor.Annotations
{
	/// <summary>
	/// Rename a controller method
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public sealed class TypeContractorNameAttribute : Attribute
	{
		/// <summary>
		/// Rename a controller method.
		/// 
		/// <para>
		/// The provided name will be used as-is, with no changes in case.
		/// It must therefore be a valid TypeScript identifier.
		/// </para>
		/// </summary>
		/// <param name="name">The new name it should have, with exact casing.</param>
		public TypeContractorNameAttribute(string name)
		{
			Name = name;
		}

		public string Name { get; }
	}
}
