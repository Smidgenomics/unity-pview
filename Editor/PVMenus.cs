// smidgens @ github

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using System;
	using UnityEditor;
	using UnityEditorInternal;
	using UnityEngine;

	internal static class PVMenus
	{
		private const int _MENU_SORT = 150;

		[MenuItem(PVConstants.ROOT_MENU_PATH + "User Settings", false, _MENU_SORT)]
		private static void OpenUserSettings()
		{
			SettingsService.OpenUserPreferences("Preferences/" + PVConstants.SETTINGS_TAB_PATH);
		}
		
		[MenuItem(PVConstants.ROOT_MENU_PATH + "Project Settings", false, _MENU_SORT)]
		private static void OpenProjectSettings()
		{
			SettingsService.OpenProjectSettings("Project/" + PVConstants.SETTINGS_TAB_PATH);
		}

		[MenuItem(PVConstants.ROOT_MENU_PATH + "Documentation", false, _MENU_SORT)]
		private static void OpenDocumentation() => Application.OpenURL(PackageManifest.Value.documentationUrl);

		private static readonly Lazy<MinimalManifest> PackageManifest = new(() =>
		{
			var packageFileGUID = PVConstants.PACKAGE_MANIFEST_GUID;
			var packageManifest = AssetDatabase.LoadAssetAtPath<PackageManifest>(AssetDatabase.GUIDToAssetPath(packageFileGUID));
			return JsonUtility.FromJson<MinimalManifest>(packageManifest.text);
		});

		// select fields from package manifest
		[Serializable]
		private struct MinimalManifest
		{
			public string documentationUrl;
		}
	}
}
