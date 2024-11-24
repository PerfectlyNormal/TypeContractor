using System;

namespace TypeContractor.Annotations
{
	/// <summary>
	/// Rename a generated client or endpoint
	/// 
	/// <para>
	/// When generating API clients, all applicable controllers will be
	/// found and generated automatically. Using this attribute is for
	/// providing a custom name in case of multiple controllers with the
	/// same name but different namespaces being present in the project.
	/// </para>
	/// 
	/// <para>
	/// When used on a controller endpoint, this will give the endpoint
	/// a different name. Useful in case of overloads that doesn't work
	/// as well in TypeScript.
	/// </para>
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public sealed class TypeContractorNameAttribute : Attribute
	{
		/// <summary>
		/// Rename a generated client or endpoint.
		/// 
		/// <para>
		/// The provided name will be used as-is, with no changes in case,
		/// or suffixes added. It must therefore be a valid TypeScript identifier.
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
