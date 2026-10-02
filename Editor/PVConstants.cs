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

	}
}