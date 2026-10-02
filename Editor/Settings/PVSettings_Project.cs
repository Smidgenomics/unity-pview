// smidgens @ github

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using UnityEditor;
	using UnityEngine;

	[FilePath("ProjectSettings/" + PVConstants.SETTINGS_FILENAME + ".asset", FilePathAttribute.Location.ProjectFolder)]
	[ExcludeFromPreset]
	internal sealed class PVSettings_Project : ScriptableSingleton<PVSettings_Project>
	{
		[Header("Default Profiles")]
		[FieldLabel("Project Menu")]
		[ProjectFile("*.pvm.json", "ProjectSettings/pview")]
		[SerializeField] internal string _menuProfile;
		
		[FieldLabel("Project Icons")]
		[ProjectFile("*.pvi.json", "ProjectSettings/pview")]
		[SerializeField] internal string _iconProfile;
		
		[Header("Deprecated")]
		[HideInInspector]
		[SerializeField] internal PVMenu _menu;
		[HideInInspector]
		[SerializeField] internal PVIcons _icons;
		
		public void Save() => Save(true);

		private void OnEnable() => hideFlags &= ~HideFlags.NotEditable;
	}
}

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using UnityEditor;

	[CustomEditor(typeof(PVSettings_Project))]
	internal sealed class _PVSettings_Project : _Inspector
	{
		private void OnDisable() => PVSettings_User.instance.Save();
	}
}