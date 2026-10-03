// smidgens @ github

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using UnityEditor;
	using UnityEngine;

	// base for settings
	internal abstract class SettingsAsset<T> : ScriptableSingleton<T> where T : ScriptableObject
	{
		public void SaveAsset() => Save(true);

		private void OnEnable() => hideFlags &= ~HideFlags.NotEditable;
	}
}

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using System;
	using System.Collections.Generic;
	using System.Reflection;
	using UnityEditor;
	using UnityEngine;

	[CustomEditor(typeof(SettingsAsset<>), true)]
	internal sealed class _SettingsAsset : _Inspector
	{
		protected override void OnBeforeFields()
		{
			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			foreach (var (l, fn) in _actions)
			{
				if (GUILayout.Button(l))
				{
					fn.Invoke();
				}
			}
			EditorGUILayout.EndHorizontal();
			DrawSeparatorIMGUI();
		}

		private IReadOnlyList<(string, Action)> _actions;
		private Action _saveFn;

		private static IReadOnlyList<(string, Action)> FindTypeActions(Type type)
		{
			const BindingFlags bf = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
			var l = new List<(string, Action)>();
			foreach (var m in type.GetMethods(bf))
			{
				if (m.ReturnType != typeof(void) || m.GetParameters().Length != 0)
				{
					continue;
				}
				var attr = m.GetCustomAttribute<StaticActionAttribute>();
				var fn = (Action)m.CreateDelegate(typeof(Action));
				l.Add((attr.label, fn));
			}
			return l;
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			_actions = FindTypeActions(target.GetType());
			const BindingFlags bf = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic;
			var m = target.GetType()!.GetMethod("SaveAsset", bf, null, Type.EmptyTypes, null);
			_saveFn = (Action)m!.CreateDelegate(typeof(Action), target);
		}

		protected override void OnDisable()
		{
			_saveFn.Invoke();
			base.OnDisable();
		}
	}
}