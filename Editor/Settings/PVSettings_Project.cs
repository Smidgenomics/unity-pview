// smidgens @ github

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using UnityEditor;
	using UnityEngine;

	/// <summary>
	/// Project settings file
	/// </summary>
	[FilePath("ProjectSettings/" + PVConstants.SETTINGS_FILENAME + ".asset", FilePathAttribute.Location.ProjectFolder)]
	[ExcludeFromPreset]
	internal sealed class PVSettings_Project : SettingsAsset<PVSettings_Project>
	{
		[Header("Project Browser")]
		[FieldLabel("Context Menu")]
		[ProjectFile("*.pvm.json", "ProjectSettings/pview")]
		[SerializeField] internal string _menuProfile;

		[FieldLabel("Icons")]
		[ProjectFile("*.pvi.json", "ProjectSettings/pview")]
		[SerializeField] internal string _iconProfile;

		[StaticAction("User Settings")]
		private static void OpenUserSettings() => ProjectView.OpenUserPrefs();
	}
}