// smidgens @ github

#pragma warning disable 0414

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using System.Text.RegularExpressions;

	public static class String_
	{
		// str contains only hex characters
		public static bool IsHexString(this string str)
		{
			return !string.IsNullOrEmpty(str) && Regex.IsMatch(str, "^([a-f]|[0-9])+$");
		}

		public static bool IsGUID32(this string str)
		{
			return str.IsHexString() && str.Length == 32;
		}
	}
}