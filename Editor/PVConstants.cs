// smidgens @ github

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using UnityEngine;

	// magic constants, lt.dan
	internal static class PVConstants
	{
		// Path to project root folder
		public static readonly string PROJECT_ROOT = Application.dataPath[..^7];

		// Tab path in Project Settings and User Preferences
		public const string SETTINGS_TAB_PATH = "Editor/SM Project View";

		// Name of settings files in Project and User settings
		public const string SETTINGS_FILENAME = "SM_ProjectView";

		// Path to menu items
		public const string ROOT_MENU_PATH = "Help/Smidgenomics/Project View/";

		// package.json
		public const string PACKAGE_MANIFEST_GUID = "97184eaee0a5be44689988835a35edc8";
	}
}