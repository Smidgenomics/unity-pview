// smidgens @ github

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using UnityEditor;
	using UnityEngine;

	/// <summary>
	/// Registers static callbacks
	/// </summary>
	[InitializeOnLoad]
	internal static class PVCallbacks
	{
		static PVCallbacks()
		{
			// hook into project browser drawing
			EditorApplication.projectWindowItemOnGUI -= ProjectView.OnBrowserGUI;
			EditorApplication.projectWindowItemOnGUI += ProjectView.OnBrowserGUI;
		}

		[SettingsProvider]
		private static SettingsProvider GetProjectSettings()
		{
			return AssetSettingsProvider.CreateProviderFromObject("Project/" + PVConstants.SETTINGS_TAB_PATH, PVSettings_Project.instance);
		}

		[SettingsProvider]
		private static SettingsProvider GetUserSettings()
		{
			var provider = AssetSettingsProvider.CreateProviderFromObject("Preferences/" + PVConstants.SETTINGS_TAB_PATH, PVSettings_User.instance);
			provider.SetScope(SettingsScope.User);
			return provider;
		}
	}
}
