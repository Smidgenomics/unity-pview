// smidgens @ github

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using UnityEngine;

	internal static class Float_
	{
		public static bool IsApproxZero(this float v)
		{
			return Mathf.Approximately(v, 0f);
		}
	}
}