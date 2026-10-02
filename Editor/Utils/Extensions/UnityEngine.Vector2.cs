// smidgens @ github

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using UnityEngine;

	/// <summary>
	/// Extensions for UnityEngine.Vector2
	/// </summary>
	internal static class Vector2_
	{
		/// <summary>
		/// Snaps values to grid (value rounding)
		/// </summary>
		public static Vector2 SnapToGrid(this Vector2 v, float snapFactor)
		{
			var x = v.x - (v.x % snapFactor);
			var y = v.y - (v.y % snapFactor);
			return new Vector2(x, y);
		}
	}
}
