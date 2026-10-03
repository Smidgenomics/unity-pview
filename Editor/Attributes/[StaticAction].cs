// smidgens @ github

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using System;

	[AttributeUsage(AttributeTargets.Method)]
	internal sealed class StaticActionAttribute : Attribute
	{
		public StaticActionAttribute(string label)
		{
			this.label = label;
		}
		internal readonly string label;
	}
}