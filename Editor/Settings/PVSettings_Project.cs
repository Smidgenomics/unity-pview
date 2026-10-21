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
		[ProjectFile("*.pvm.json", "ProjectSettings/pview", PVConstants.EX_FOLDER_PATH)]
		[SerializeField] internal string _menuProfile = PVConstants.EX_FOLDER_PATH + "/example.pvm.json";

		[FieldLabel("Icons")]
		[ProjectFile("*.pvi.json", "ProjectSettings/pview", PVConstants.EX_FOLDER_PATH)]
		[SerializeField] internal string _iconProfile = PVConstants.EX_FOLDER_PATH + "/example.pvi.json";

		[StaticAction("User Settings")]
		private static void OpenUserSettings() => ProjectView.OpenUserPrefs();
	}
}