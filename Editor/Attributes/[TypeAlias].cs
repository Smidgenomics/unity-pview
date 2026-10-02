// smidgens @ github

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using System;

	/// <summary>
	/// Gives a type a shorthand for serialization purposes
	/// </summary>
	[AttributeUsage(AttributeTargets.Class|AttributeTargets.Struct, AllowMultiple = true)]
	internal sealed class TypeAliasAttribute : Attribute
	{
		public TypeAliasAttribute(string alias)
		{
			this.alias = alias;
		}
		internal string alias { get; }
	}
}