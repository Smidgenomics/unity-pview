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

		public void Save() => Save(true);

		private void OnEnable() => hideFlags &= ~HideFlags.NotEditable;
	}
}

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using UnityEditor;
	using UnityEngine;

	[CustomEditor(typeof(PVSettings_Project))]
	internal sealed class _PVSettings_Project : _Inspector
	{
		protected override void OnBeforeFields()
		{
			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("User Settings"))
			{
				SettingsService.OpenUserPreferences("Preferences/" + PVConstants.SETTINGS_TAB_PATH);
			}
			EditorGUILayout.EndHorizontal();
		}

		private void OnDisable() => PVSettings_Project.instance.Save();
	}
}