// smidgens @ github

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using System;

	/// <summary>
	/// Add to string field to automatically inject variables
	/// </summary>
	[AttributeUsage(AttributeTargets.Field)]
	public sealed class InjectVariablesAttribute : Attribute
	{
		
	}
}