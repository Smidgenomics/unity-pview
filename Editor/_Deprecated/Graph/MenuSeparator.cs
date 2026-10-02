// smidgens @ github

namespace Smidgenomics.Unity.ProjectView.Editor
{
	/// <summary>
	/// Menu separator type
	/// </summary>
	[System.ComponentModel.DisplayName("Separator")]
	internal class MenuSeparator : MenuNode
	{
		protected override string GetLabel() => "- - - - ";
	}
}