// smidgens @ github

#pragma warning disable 0414

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using System.Collections.Generic;
	using UnityEditor;
	using UnityEditor.UIElements;
	using UnityEngine;
	using UnityEngine.UIElements;

	// basic custom inspector
	internal abstract class _Inspector : Editor
	{
		public override VisualElement CreateInspectorGUI()
		{
			var root = new IMGUIContainer();
			// not sure if this is entirely safe...
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

		protected virtual void OnBeforeFields()
		{
			
		}
		
		protected virtual void OnAfterFields()
		{
			
		}

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
	}


}