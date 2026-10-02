// smidgens @ github

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using System;
	using UnityEditor;
	using UnityEditorInternal;
	using UnityEngine;

	internal static class PVMenus
	{
		[MenuItem("Help/Smidgenomics/Project View/Online Documentation")]
		private static void OpenDocumentation()
		{
			var url = PackageManifest.Value.documentationUrl;
			if (!string.IsNullOrEmpty(url))
			{
				Application.OpenURL(url);
			}
		}

		private static readonly Lazy<MinimalManifest> PackageManifest = new(() =>
		{
			var packageFileGUID = "97184eaee0a5be44689988835a35edc8";
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
