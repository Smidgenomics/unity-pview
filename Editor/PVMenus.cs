// smidgens @ github

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using System;
	using UnityEditor;
	using UnityEditorInternal;
	using UnityEngine;

	// menu items
	internal static class PVMenus
	{
		[MenuItem(PVConstants.ROOT_MENU_PATH + "Documentation", false, PVConstants.MENU_SORT - 20)]
		private static void OpenDocumentation() => Application.OpenURL(PackageManifest.Value.documentationUrl);
		
		[MenuItem(PVConstants.ROOT_MENU_PATH + "User Settings", false, PVConstants.MENU_SORT)]
		private static void OpenUserSettings() => ProjectView.OpenUserPrefs();

		[MenuItem(PVConstants.ROOT_MENU_PATH + "Project Settings", false, PVConstants.MENU_SORT)]
		private static void OpenProjectSettings() => ProjectView.OpenProjectPrefs();

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
