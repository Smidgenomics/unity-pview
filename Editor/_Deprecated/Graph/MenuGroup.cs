// smidgens @ github

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using UnityEngine;

	/// <summary>
	/// Submenu/group node
	/// </summary>
	[System.ComponentModel.DisplayName("Group")]
	internal sealed class MenuGroup : MenuNode
	{
		protected override int GetOutputCount() => 1;
		protected override Color GetColor() => new (0.349f, 0.074f, 0.925f);
	}
}
