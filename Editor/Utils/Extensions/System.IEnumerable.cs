// smidgens @ github

namespace Smidgenomics.Unity.ProjectView.Editor
{
	/// <summary>
	/// Extensions for arrays
	/// </summary>
	internal static class Array_
	{
		/// <summary>
		/// Checks if array is equal in size, refs, and ordering
		/// </summary>
		public static bool IsSameAs<T>(this T[] arr, in T[] otherArr) where T : class
		{
			if (arr.Length != otherArr.Length) { return false; }
			for (var i = 0; i < arr.Length; i++)
			{
				if (arr[i] != otherArr[i]) { return false; }
			}
			return true; ;
		}
	}
}