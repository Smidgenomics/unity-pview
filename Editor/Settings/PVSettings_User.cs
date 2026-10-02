// smidgens @ github

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using UnityEditor;
	using UnityEngine;
	using UnityEngine.Serialization;

	[FilePath("UserSettings/" + PVConstants.SETTINGS_FILENAME + ".asset", FilePathAttribute.Location.ProjectFolder)]
	[ExcludeFromPreset]
	internal sealed class PVSettings_User : ScriptableSingleton<PVSettings_User>
	{
		[Header("Profiles (User)")]
		[FieldLabel("Menu")]
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

		[FormerlySerializedAs("_keyboardModifiers")]
		[Header("Modifiers (Context Menu)")]
		[Expand(innerOnly:true)]
		[SerializeField] internal MenuModifiers _modifierBehaviours = MenuModifiers.GetDefault();

		public void Save() => Save(true);

		private void OnEnable() => hideFlags &= ~HideFlags.NotEditable;
	}
}


namespace Smidgenomics.Unity.ProjectView.Editor
{
	using UnityEditor;

	[CustomEditor(typeof(PVSettings_User))]
	internal sealed class _PVSettings_User : _Inspector
	{
		private void OnDisable() => PVSettings_User.instance.Save();
	}
}