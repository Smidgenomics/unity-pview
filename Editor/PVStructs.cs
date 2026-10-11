// smidgens @ github

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using System;

	// overrideable value
	[Serializable]
	internal struct OptionalValue<T>
	{
		public bool enabled;
		public T value;
	}
}

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using UnityEngine;

	internal struct LoadedIcon
	{
		public Texture2D tex;
		public Color tint;
		public Rect pos;
		public Rect uv;
		public SkinPick<Color> bgColor;
		public bool floatRight;
	}
}

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using Newtonsoft.Json;

	internal struct IconDefaults
	{
		[JsonProperty] public IconSettings folderIcon;
	}
}

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using UnityEditor;

	// selects value based on editor skin
	internal readonly struct SkinPick<T>
	{
		public T Value => EditorGUIUtility.isProSkin ? _dValue : _lValue;

		public SkinPick(T lightValue, T darkValue)
		{
			_dValue = darkValue;
			_lValue = lightValue;
		}

		public static implicit operator T(SkinPick<T> v) => v.Value;
		public static implicit operator SkinPick<T>((T, T) val) => new(val.Item1, val.Item2);

		private readonly T _dValue, _lValue;
	}
}

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using System;

	[Serializable]
	internal struct MenuModifiers
	{
		public OptionalValue<EOverrideBehaviour> ctrl, shift, alt;

		public static MenuModifiers GetDefault() => new()
		{
			ctrl = new OptionalValue<EOverrideBehaviour>
			{
				enabled = true,
				value = EOverrideBehaviour.UnityDefault
			},
			shift = new OptionalValue<EOverrideBehaviour>
			{
				enabled = true,
				value = EOverrideBehaviour.ProjectProfile
			}
		};

		public readonly bool AreAnyEnabled()
		{
			return ctrl.enabled || shift.enabled || alt.enabled;
		}
	}
}

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using System.Collections.Generic;
	using System.IO;

	internal readonly struct FileEditCheck
	{
		public bool AnyEdited()
		{
			if (_paths != null)
			{
				foreach (var (path, time) in _paths)
				{
					if (File.GetLastWriteTimeUtc(path).ToFileTime() != time)
					{
						return true;
					}
				}
			}
			return false;
		}

		public FileEditCheck(IEnumerable<string> filePaths)
		{
			_paths = new List<(string, long)>();
			foreach (var relPath in filePaths)
			{
				var absPath = $"{PVConstants.PROJECT_ROOT}/{relPath}";
				if (!File.Exists(absPath))
				{
					continue;
				}
				var time = File.GetLastWriteTimeUtc(absPath).ToFileTime();
				_paths.Add((absPath, time));
			}
		}
		// absolute path/edit time
		private readonly List<(string, long)> _paths;
	}
}

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using UnityEditor;
	using UnityEngine;

	[CustomPropertyDrawer(typeof(OptionalValue<>))]
	internal sealed class _OptionalValue : PropertyDrawer
	{
		public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
		{
			var vProp = prop.FindPropertyRelative(nameof(OptionalValue<byte>.value));
			return EditorGUI.GetPropertyHeight(vProp, label);
		}

		public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, prop);
			var eProp = prop.FindPropertyRelative(nameof(OptionalValue<byte>.enabled));
			var vProp = prop.FindPropertyRelative(nameof(OptionalValue<byte>.value));
			var lWidth = EditorGUIUtility.labelWidth;
			var lRect = position.SliceLeft(lWidth);
			position.SliceLeft(EditorGUIUtility.standardVerticalSpacing);
			var toggleRect = lRect.SliceLeft(EditorGUIUtility.singleLineHeight);
			position.SliceLeft(EditorGUIUtility.standardVerticalSpacing);
			EditorGUI.PropertyField(toggleRect, eProp, GUIContent.none);
			var tEnabled = GUI.enabled;
			GUI.enabled = eProp.boolValue;
			EditorGUI.LabelField(lRect, label);
			var tIndent = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;
			EditorGUI.PropertyField(position, vProp, GUIContent.none);
			EditorGUI.indentLevel = tIndent;
			GUI.enabled = tEnabled;
			EditorGUI.EndProperty();
		}
	}
	
}