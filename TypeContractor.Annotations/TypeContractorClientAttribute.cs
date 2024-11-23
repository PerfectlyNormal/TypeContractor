using System;

namespace TypeContractor.Annotations
{
	/// <summary>
	/// Annotate a controller to generate an automatic API client from.
	/// 
	/// <para>
	/// When generating API clients, all applicable controllers will be
	/// found and generated automatically. This attribute is just for
	/// providing a custom name in case of multiple controllers with the
	/// same name but different namespaces being present in the project.
	/// </para>
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public sealed class TypeContractorClientAttribute : Attribute
	{
		/// <summary>
		/// Provide a custom name for the generated client
		/// </summary>
		/// <param name="name">
		/// Name of the client. No suffix will be added,
		/// the provided name is used as-is.
		/// </param>
		public TypeContractorClientAttribute(string name)
		{
			Name = name;
		}

		public string Name { get; }
	}
}
