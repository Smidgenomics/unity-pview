// smidgens @ github

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using UnityEngine;

	// misc fixed values
	internal static class UnityConstants
	{
		public static readonly SkinPick<Color> BorderColor = new
		(
			PVParse.ParseColor(COLOR_BORDER_L, default),
			PVParse.ParseColor(COLOR_BORDER_D, default)
		);

		public static readonly SkinPick<Color> BrowserColor = new
		(
			PVParse.ParseColor(COLOR_BROWSER_BG_L, default),
			PVParse.ParseColor(COLOR_BROWSER_BG_D, default)
		);

		// selected asset color
		public const string COLOR_ACTIVE_D = "#3d6091";
		public const string COLOR_ACTIVE_L = "#3d80df";
		// project browser background
		public const string COLOR_BROWSER_BG_D = "#333333";
		public const string COLOR_BROWSER_BG_L = "#bebebe";

		// dividers, stuff like that
		public const string COLOR_BORDER_D = "#1A1A1A";
		public const string COLOR_BORDER_L = "#7F7F7F";
		// project browser, icon size considered "large"
		public const float BROWSER_ICON_LG = 88f;
		
		
	}
}