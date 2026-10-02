// smidgens @ github

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using System;
	using UnityEngine;

	[AttributeUsage(AttributeTargets.Field)]
	internal sealed class ProjectFileAttribute : PropertyAttribute
	{
		public ProjectFileAttribute(string pattern, params string[] folders)
		{
			this.pattern = pattern;
			this.folders = folders ?? Array.Empty<string>();
		}

		public readonly string pattern;
		public readonly string[] folders;
	}
}

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using UnityEditor;
	using UnityEngine;
	using UnityEngine.UIElements;

	[CustomPropertyDrawer(typeof(ProjectFileAttribute))]
	internal sealed class _ProjectFile : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EnsureInit();

			if (label != GUIContent.none)
			{
				position = EditorGUI.PrefixLabel(position, label);
			}

			var editBtnRect = position.SliceRight(position.height);
			position.SliceRight(EditorGUIUtility.standardVerticalSpacing);
			var currentValue = property.stringValue;
			var btnLabel = GetButtonLabel(currentValue);
			if (EditorGUI.DropdownButton(position, btnLabel, FocusType.Keyboard))
			{
				// var dd = new GenericDropdownMenu();
				var m = new GenericMenu();

				var opts = _options.Value;

				var lastPrefix = opts.Count > 1 ? opts[1].Item2[0] : '\0';
				var i = -1;
				foreach (var (l, v) in _options.Value)
				{
					i++;
					if (v.Length > 0 && lastPrefix != v[0])
					{
						m.AddSeparator(string.Empty);
						lastPrefix = v[0];
					}
					
					m.AddItem(l, v == currentValue, () =>
					{
						property.stringValue = v;
						property.serializedObject.ApplyModifiedProperties();
					});

					if (i == 0 && opts.Count > 1)
					{
						m.AddSeparator(string.Empty);
					}
					
					
					
				}
				m.DropDown(position);
				var w = EditorWindow.focusedWindow;

				// dd.DropDown(position, w.rootVisualElement, DropdownMenuSizeMode.Auto);
			}

			var tEnabled = GUI.enabled;
			GUI.enabled = btnLabel == _btnLabel;
			if (GUI.Button(editBtnRect, _editIcon, EditorStyles.iconButton))
			{
				OpenFile(currentValue);
			}
			GUI.enabled = tEnabled;

		}

		private Lazy<List<(GUIContent, string)>> _options;

		private readonly GUIContent _editIcon = new (EditorGUIUtility.IconContent("editicon.sml"))
		{
			tooltip = "Edit profile..."
		};

		private void OpenFile(string relativePath)
		{
			var path = $"{_PROJECT_ROOT}/{relativePath}";
			EditorUtility.OpenWithDefaultApp(path);
		}
		
		private GUIContent GetButtonLabel(string value)
		{
			if (string.IsNullOrEmpty(value))
			{
				return _LABEL_NONE;
			}

			if (!CheckProjectFileExists(value))
			{
				return _MISS_LABEL;
			}

			_btnLabel.text = value;
			return _btnLabel;
		}

		private static readonly string _PROJECT_ROOT = Application.dataPath[..^7];
		private readonly GUIContent _btnLabel = new(string.Empty);
		private static readonly GUIContent _MISS_LABEL = new("(missing)");
		private static readonly GUIContent _LABEL_NONE = new("(none)");

		private void EnsureInit()
		{
			if (_options != null)
			{
				return;
			}
			_options = new(CreateOptions);
		}

		private static bool CheckProjectFileExists(string relativePath)
		{
			return File.Exists($"{_PROJECT_ROOT}/{relativePath}");
		}
		
		private List<(GUIContent,string)> CreateOptions()
		{
			var l = new List<(GUIContent,string)>
			{
				(new GUIContent("(none)"), "")
			};

			var attr = (attribute as ProjectFileAttribute)!;
			var pattern = attr.pattern;

			foreach (var folder in attr.folders)
			{
				var absPath = Path.Combine(_PROJECT_ROOT, folder);

				if (!Directory.Exists(absPath))
				{
					continue;
				}

				foreach (var file in Directory.GetFiles(absPath, pattern))
				{
					var relPath = MakePathRelative(_PROJECT_ROOT, file);
					var pLabel = relPath.Substring(relPath.LastIndexOf('/') + 1);
					l.Add((new GUIContent(pLabel), relPath));
				}
			}

			// todo: add paths relative to project root

			return l;
		}

		private static string MakePathRelative(string rootDir, string path)
		{
			return path.Substring(rootDir.Length + 1).Replace('\\', '/');

		}
		
		
		
		
	}
}