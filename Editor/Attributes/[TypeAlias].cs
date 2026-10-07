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
		public TypeAliasAttribute(string alias, bool obsolete = false)
		{
			this.alias = alias;
			this.obsolete = obsolete;
		}
		internal string alias { get; }

		// alias should work but give off warning if referenced
		internal bool obsolete { get; }
	}
}