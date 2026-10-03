// smidgens @ github

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using System.Text.RegularExpressions;

	public static class String_
	{
		private const string _HEX_REGEX = "^([a-f]|[0-9])+$";

		// str contains only hex characters
		public static bool IsHexString(this string str)
		{
			return !string.IsNullOrEmpty(str) && Regex.IsMatch(str, _HEX_REGEX);
		}

		public static bool IsGUID32(this string str)
		{
			return str.IsHexString() && str.Length == 32;
		}

		public static bool MatchWildcard(this System.Collections.Generic.IEnumerable<string> patternList, string item)
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