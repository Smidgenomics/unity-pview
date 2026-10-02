// smidgens @ github

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using UnityEditor;

	/// <summary>
	/// Extensions for UnityEditor.EditorWindow
	/// </summary>
	internal static class EditorWindow_
	{
		/// <summary>
		/// Checks if given editor window has mouse over
		/// </summary>
		public static bool IsCurrentlyMoused(this EditorWindow w) => EditorWindow.mouseOverWindow == w;
	}
}