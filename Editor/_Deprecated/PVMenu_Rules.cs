// smidgens @ github

#pragma warning disable CS0618 // Type or member is obsolete
namespace Smidgenomics.Unity.ProjectView.Editor
{
	using UnityEngine;
	using System;
	using System.Linq;
	using UnityEngine.Serialization;
	using System.Collections.Generic;
	using UnityEditor;

	/// <summary>
	/// Custom menu using pattern matching
	/// </summary>
	// [CreateAssetMenu(menuName = PVConstants.CREATE_ROOT + "Menu/Pattern")]
	[Obsolete("Replaced with JSON profiles")]
	internal sealed class PVMenu_Rules : PVMenu
	{
		[SerializeField] internal string _menuTitle = "";
		
		[Expand(innerOnly:true)]
		[SerializeField] internal Settings _settings;

		[Inline]
		[FormerlySerializedAs("_hide")]
		[FormerlySerializedAs("_match")]
		[SerializeField, HideInInspector] internal MatchRule[] _rules = Array.Empty<MatchRule>();
		[Inline]
		[SerializeField, HideInInspector] internal MoveRule[] _move = Array.Empty<MoveRule>();

		private struct MenuItem
		{
			public string path;
			public string exePath;
			public GUIContent label;
			public bool separator;
			public GenericMenu.MenuFunction fn;
		}
		
		public override GenericMenu GetMenu()
		{
			return MenuFromItems(GetItems());
		}

		private IEnumerable<MenuItem> GetItems()
		{
			var items =
			UnityUtility.GetSubmenus("Assets")
			.Where(x =>
			{
				var cmpPath = x.Substring(7);

				foreach (var r in ALWAYS_SKIPPED_MENUS)
				{
					if (cmpPath.StartsWith(r)) { return false; }
				}
				if (!Match(cmpPath)) { return false; }
				return true;
			})
			.ToArray();

			var options = new List<MenuItem>();

			for (var i = 0; i < items.Length; i++)
			{
				var path = items[i];
				var cmpPath = path.Substring(7);

				var label =
				MatchMove(cmpPath, out var newLabel)
				? newLabel
				: path.Substring(7);

				var nextPath = i < items.Length - 1 ? items[i + 1] : null;
				var exePath = $"{path}";

				options.Add(new MenuItem
				{
					exePath = exePath,
					label = new GUIContent(label),
					//separator = separator,
					path = path,
					fn = () => UnityUtility.ExecuteMenu(exePath)
				});
			}
			return options;
		}

		private static readonly string[] ALWAYS_SKIPPED_MENUS =
		{
			"Create/Playables" // these bug unity out for some reason
		};
		
		private GenericMenu MenuFromItems(IEnumerable<MenuItem> items)
		{
			var m = new GenericMenu();

			m.allowDuplicateNames = true;

			if (!string.IsNullOrEmpty(_menuTitle))
			{
				m.AddDisabledItem(new GUIContent($"� {_menuTitle} �"));
				m.AddSeparator("");
			}

			foreach (var item in items)
			{
				if (item.separator) { continue; }
				// if it weren't for this check, the menu would be cachable
				if (UnityUtility.CanExecuteMenu(item.exePath))
				{
					m.AddItem(item.label, false, item.fn);
				}
				else
				{
					m.AddDisabledItem(item.label);
				}
			}
			return m;
		}

		internal enum RuleMode
		{
			Exclude,
			Include
		}

		[Serializable]
		internal struct Settings
		{
			public RuleMode mode;
		}

		private bool MatchMove(in string p, out string newPath)
		{
			newPath = null;
			if (string.IsNullOrEmpty(p)) { return false; }
			foreach(var r in _move)
			{
				if(Wildcard.IsMatch(p, r.pattern))
				{
					newPath = Move(p, r.output);
					return true;
				}
			}
			return false;
		}

		private static string Move(in string path, in string newPrefix)
		{
			if (newPrefix.Length == 0)
			{
				return path.Split('/').LastOrDefault();
			}
			return newPrefix + "/" + path.Split('/').LastOrDefault();
		}

		private bool Match(string p)
		{
			if (_settings.mode == RuleMode.Exclude)
			{
				return MatchOrDefault(p, false, true);
			}
			return MatchOrDefault(p, true, false);
		}

		private bool MatchOrDefault(string p, bool matchResult, bool def)
		{
			foreach (var r in _rules)
			{
				if (Wildcard.IsMatch(p, r.pattern))
				{
					return matchResult;
				}
			}
			return def;
		}

		[Serializable]
		internal sealed class MatchRule
		{
			public string pattern;
		}

		[Serializable]
		internal sealed class MoveRule
		{
			public string pattern;
			public string output;
		}
	}
}

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using UnityEngine;
	using UnityEditor;
	using System;
	using System.Collections.Generic;
	using UnityEditorInternal;

	[CustomEditor(typeof(PVMenu_Rules))]
	internal sealed class _PVMenu_Rules : _Inspector
	{
		protected override bool ShouldHideOpenButton() => true;

		protected override void OnAfterFields()
		{
			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Rules", EditorStyles.whiteLargeLabel);
			serializedObject.UpdateIfRequiredOrScript();
			foreach (var l in _lists)
			{
				l.DoLayoutList();
			}
			serializedObject.ApplyModifiedProperties();

			EditorGUILayout.LabelField("Debug", EditorStyles.largeLabel);
			EditorGUILayout.BeginHorizontal();
			foreach (var (l, fn) in _DEBUG_ACTIONS)
			{
				if (string.IsNullOrEmpty(l))
				{
					GUILayout.FlexibleSpace();
					continue;
				}
				if (GUILayout.Button(l))
				{
					fn.Invoke(target as PVMenu_Rules);
				}
			}
			EditorGUILayout.EndHorizontal();
		}

		private readonly List<ReorderableList> _lists = new();

		// label, fn, dev only
		private static readonly (string, Action<PVMenu_Rules>)[] _DEBUG_ACTIONS =
		{
// #if SM_DEV || SM_DEBUG
// 			("Clear Cache", _ => ProjectView.ClearCache()),
// #endif
			default,
			("Preview", _ => Debug.Log("Preview N/I")),
		};

		private static readonly (string, Action<Rect, ReorderableList>)[] _LIST_FIELDS =
		{
			(nameof(PVMenu_Rules._rules), (r, l) =>
			{
				var target = (PVMenu_Rules)l.serializedProperty.serializedObject.targetObject;
				var include = target._settings.mode == PVMenu_Rules.RuleMode.Include;
				var prefix = include ? "Include" : "Exclude";
				GUI.Label(r, $"{prefix} ({l.serializedProperty.arraySize})");
			}),
			(nameof(PVMenu_Rules._move), (r, l) =>
			{
				GUI.Label(r, $"Move ({l.serializedProperty.arraySize})");
			}),
		};

		protected override void OnEnable()
		{
			base.OnEnable();
			foreach (var (fName, labelFn) in _LIST_FIELDS)
			{
				var listProp = serializedObject.FindProperty(fName);
				if (!listProp.isArray)
				{
					continue;
				}
				var l = new ReorderableList(serializedObject, listProp, true, true, true, true);
				l.drawHeaderCallback = r =>
				{
					labelFn.Invoke(r, l);
				};
				l.elementHeightCallback = i => EditorGUI.GetPropertyHeight(listProp.GetArrayElementAtIndex(i));
				
				l.drawElementCallback = (rect, i, _, _) =>
				{
					EditorGUI.PropertyField(rect, listProp.GetArrayElementAtIndex(i), GUIContent.none);
				};
				
				_lists.Add(l);
			}
		}

		private void OnDisable()
		{
			// cleanup if needed
		}
	}
}