// smidgens @ github

#pragma warning disable 0414

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using System.Collections.Generic;
	using UnityEditor;
	using UnityEngine;
	using UnityEngine.UIElements;

	// basic custom inspector
	internal abstract class _Inspector : Editor
	{
		public override VisualElement CreateInspectorGUI()
		{
			var root = new IMGUIContainer();
			// not sure if this is entirely robust, we shall see...
			root.onGUIHandler = OnInspectorGUI;
			return root;
		}

		public sealed override void OnInspectorGUI()
		{
			// no props
			OnBeforeFields();
			if (_props != null)
			{
				serializedObject.UpdateIfRequiredOrScript();
				foreach (var p in _props)
				{
					EditorGUILayout.PropertyField(p);
				}
				serializedObject.ApplyModifiedProperties();
			}
			OnAfterFields();
		}

		private IReadOnlyList<SerializedProperty> _props;

		protected static void DrawSeparatorIMGUI()
		{
			GUILayout.Space(EditorGUIUtility.standardVerticalSpacing);
			var r = EditorGUILayout.GetControlRect(GUILayout.Height(1f));
			EditorGUI.DrawRect(r, UnityConstants.BorderColor);
			GUILayout.Space(EditorGUIUtility.standardVerticalSpacing);
		}

		protected virtual void OnBeforeFields(){}
		
		protected virtual void OnAfterFields(){}

		protected virtual void OnEnable()
		{
			var props = new List<SerializedProperty>();
			var fields = target.GetType().FindInspectorFields<UnityEngine.Object>();

			foreach (var f in fields)
			{
				var p = serializedObject.FindProperty(f.Name);
				if (p != null)
				{
					props.Add(p);
				}
			}
			_props = props;
		}

		protected virtual void OnDisable()
		{
			
		}
	}


}