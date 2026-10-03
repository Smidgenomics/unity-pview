// smidgens @ github

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using UnityEditor;
	using UnityEngine;

	/// <summary>
	/// User settings file
	/// </summary>
	[FilePath("UserSettings/" + PVConstants.SETTINGS_FILENAME + ".asset", FilePathAttribute.Location.ProjectFolder)]
	[ExcludeFromPreset]
	internal sealed class PVSettings_User : SettingsAsset<PVSettings_User>
	{
		[Header("Profiles (User)")]
		[FieldLabel("Context Menu")]
		[ProjectFile("*.pvm.json", "ProjectSettings/pview", "UserSettings/pview")]
		[SerializeField] internal string _menuProfile;

		[FieldLabel("Icons")]
		[ProjectFile("*.pvi.json", "ProjectSettings/pview", "UserSettings/pview")]
		[SerializeField] internal string _iconProfile;

		[Header("Base Behaviour")]
		[FieldLabel("Menu")]
		[SerializeField] internal EOverrideBehaviour _defaultMenu = EOverrideBehaviour.UserProfile;
		[FieldLabel("Icons")]
		[SerializeField] internal EOverrideBehaviour _defaultIcons = EOverrideBehaviour.UserProfile;

		[Header("Modifiers (Context Menu)")]
		[Expand(innerOnly:true)]
		[SerializeField] internal MenuModifiers _modifierBehaviours = MenuModifiers.GetDefault();

		[StaticAction("Project Settings")]
		private static void OpenProjectSettings() => ProjectView.OpenProjectPrefs();
	}
}