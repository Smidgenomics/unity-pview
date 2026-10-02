// smidgens @ github

#pragma warning disable CS0618 // Type or member is obsolete
namespace Smidgenomics.Unity.ProjectView.Editor
{
	using UnityEngine;
	using System;
	using UnityEditor;

	[Obsolete("Replaced with JSON profiles")]
	internal sealed class PVIcons_Rules : PVIcons
	{
		public override void Draw(string guid, in Rect pos)
		{
			AtlasSprite icon = default;

			if(MatchIcon(guid, ref icon))
			{
				IconGUI.DrawIcon(pos, icon.Texture, new Rect(icon.Offset, icon.Size));
			}
		}

		[SerializeField] internal IcoRule[] _rules = Array.Empty<IcoRule>();

		private bool MatchIcon(string guid, ref AtlasSprite icon)
		{
			var path = AssetDatabase.GUIDToAssetPath(guid);
			var ctx = new MatchContext
			{
				guid = guid,
				assetPath = path,
				isFolder = AssetDatabase.IsValidFolder(path),
			};

			foreach (var r in _rules)
			{
				if (MatchOne(r, ctx))
				{
					icon = r.icon;
					return true;
				}
			}

			return false;
		}

		internal AtlasSprite SpriteAt(int i)
		{
			return i > -1 && i < _rules.Length ? _rules[i].icon : default;
		}

		internal enum RuleTarget { Folder, Asset, }

		internal struct MatchContext
		{
			public string guid;
			public string assetPath;
			public bool isFolder;
		}

		private bool MatchOne(in IcoRule rule, in MatchContext ctx)
		{
			if(rule.type == RuleType.Asset)
			{
				return rule.asset == ctx.guid;
			}

			// path/type
			if (string.IsNullOrEmpty(rule.pattern))
			{
				return false;
			}

			if(rule.type == RuleType.Path)
			{
				if(ctx.isFolder != rule.folder) { return false; }

				return Wildcard.IsMatch(ctx.assetPath, rule.pattern);
			}

			return false;
		}

		internal enum RuleType
		{
			Asset,
			Path,
			Type
		}

		[Serializable]
		internal struct IcoRule
		{
			public string label;
			public RuleType type;
			public string pattern;
			public AtlasSprite icon;
			public bool mute;
			[AssetGUID]
			public string asset;
			public bool folder;
		}

	}
}

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using UnityEngine;
	using UnityEditor;
	using UnityEditorInternal;

	[CustomEditor(typeof(PVIcons_Rules))]
	internal sealed class _PVProfile_Icons : Editor
	{
		public override void OnInspectorGUI()
		{
			serializedObject.UpdateIfRequiredOrScript();

			if(_list.count > 0 && _list.index < 0 || _list.index > _list.count)
			{
				_list.index = 0;
			}

			EditorGUILayout.Space(2f);
			_list.DoLayoutList();
			GUILayout.Space(4f);
			DrawSelected();
			serializedObject.ApplyModifiedProperties();
		}

		protected override bool ShouldHideOpenButton() => true;

		private ReorderableList _list;

		private void OnEnable()
		{
			var rulesProp = serializedObject.FindProperty(nameof(PVIcons_Rules._rules));

			_list = new ReorderableList(serializedObject, rulesProp);

			_list.drawElementCallback = DrawListElement;

			_list.elementHeight = EditorGUIUtility.singleLineHeight;

			_list.drawHeaderCallback = r =>
			{
				EditorGUI.LabelField(r, _list.serializedProperty.displayName);
			};

			_list.onAddCallback = l =>
			{
				var ni = _list.serializedProperty.arraySize;
				_list.serializedProperty.arraySize++;
				var newElement = _list.serializedProperty.GetArrayElementAtIndex(ni);
				var size = newElement.FindPropertyRelative("icon._size");

				if (Mathf.Approximately(size.vector2Value.x, 0f))
				{
					size.vector2Value = Vector2.one;
				}
			};
		}

		private static readonly string[][] _typeProps =
		{
			new []
			{
				"asset"
			},
			new []
			{
				"pattern",
				"folder"
			},
			new []
			{
				"pattern",
			}
		};

		private void DrawListElement(Rect pos, int i, bool f, bool a)
		{
			/*
			- type
			- icon
			*/

			var prop = _list.serializedProperty.GetArrayElementAtIndex(i);

			var icon = ((PVIcons_Rules)target).SpriteAt(i);

			if (icon.Texture)
			{
				var icoPos = pos.SliceRight(pos.height);
				icoPos.Resize(-1f);
				icon.Draw(icoPos);
				pos.SliceRight(2f);
			}

			var type = prop.FindPropertyRelative("type");

			string label = (prop.FindPropertyRelative("label")).stringValue;

			if (string.IsNullOrEmpty(label))
			{
				var sb = new System.Text.StringBuilder("");

				var typeName = type.enumNames[type.enumValueIndex];

				sb.Append(typeName);
				sb.Append(": ");

				if (type.enumValueIndex == 0)
				{
					var guid = prop.FindPropertyRelative("asset").stringValue;
					sb.Append(string.IsNullOrEmpty(guid) ? "<unset>" : guid);
				}

				if (type.enumValueIndex == 1)
				{
					var pattern = prop.FindPropertyRelative("pattern").stringValue;
					sb.Append(string.IsNullOrEmpty(pattern) ? "<unset>" : pattern);
				}
				label = sb.ToString();
			}

			EditorGUI.LabelField(pos, label, EditorStyles.miniLabel);
		}

		private void DrawSelected()
		{
			var i = _list.index;
			if (i < 0 || i >= _list.count)
			{
				return;
			}
			var prop = _list.serializedProperty.GetArrayElementAtIndex(i);
			var icon = prop.FindPropertyRelative(nameof(PVIcons_Rules.IcoRule.icon));
			var type = prop.FindPropertyRelative(nameof(PVIcons_Rules.IcoRule.type));
			var label = prop.FindPropertyRelative(nameof(PVIcons_Rules.IcoRule.label));

			EditorGUILayout.PropertyField(label);
			EditorGUILayout.Space(5f);

			EditorGUILayout.PropertyField(type);

			if (!_typeProps.IsOutOfBounds(type.enumValueIndex))
			{
				PropertyAll(prop, _typeProps[type.enumValueIndex]);
			}
			EditorGUILayout.Space(5f);
			EditorGUILayout.PropertyField(icon);

		}

		private void PropertyAll(SerializedProperty prop, string[] childProps)
		{
			foreach (var pn in childProps)
			{
				EditorGUILayout.PropertyField(prop.FindPropertyRelative(pn));
			}
		}


	}
}