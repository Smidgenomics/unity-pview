// smidgens @ github

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using System.Collections;

	/// <summary>
	/// Extensions for arrays
	/// </summary>
	internal static class IList_
	{
		/// <summary>
		/// Check if index is out of bounds
		/// </summary>
		public static bool IsOutOfBounds(this IList arr, in int i) => i < 0 || i >= arr.Count;
	}
}