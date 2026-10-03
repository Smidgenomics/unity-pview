// smidgens @ github

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using System.Text.RegularExpressions;

	/// <summary>
	/// Regex that matches strings by wildcard pattern
	/// </summary>
	internal sealed class Wildcard : Regex
	{
		public Wildcard(string pattern) : base(WildcardToRegex(pattern)) { }

		public Wildcard(string pattern, RegexOptions options) : base(WildcardToRegex(pattern), options) { }

		public static bool IsMatch(in string input, in string pattern)
		{
			return Regex.IsMatch(input, WildcardToRegex(pattern));
		}
		public static string WildcardToRegex(string pattern)
		{
			return "^" + Escape(pattern)
			.Replace("\\*", ".*")
			.Replace("\\?", ".") + "$";
		}
	}

}