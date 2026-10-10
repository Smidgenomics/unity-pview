// smidgens @ github

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using System.Text.RegularExpressions;

	public static class String_
	{
		private static readonly Regex _HEX_REGEX = new ("^([a-f]|[0-9])+$");

		// str contains only hex characters
		public static bool IsHexString(this string str)
		{
			return !string.IsNullOrEmpty(str) && _HEX_REGEX.IsMatch(str);
		}

		public static bool IsGUID32(this string str)
		{
			return str is { Length: 32 } && str.IsHexString();
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