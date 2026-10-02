// smidgens @ github

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using System.Collections.Generic;

	/// <summary>
	/// Extensions for arrays
	/// </summary>
	internal static class IEnumerable_
	{
		public static bool MatchWildcard(this IEnumerable<string> patternList, string item)
		{
			if (patternList == null)
			{
				return false;
			}
			foreach (var pattern in patternList)
			{
				if (Wildcard.IsMatch(item, pattern))
				{
					return true;
				}
			}
			return false;
		}
	}
}