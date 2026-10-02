// smidgens @ github

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using System;
	using UnityEngine;

	/// <summary>
	/// Override field label
	/// </summary>
	[AttributeUsage(AttributeTargets.Field)]
	internal sealed class FieldLabelAttribute : PropertyAttribute
	{
		internal string label { get; }

		public FieldLabelAttribute(string label)
		{
			this.label = label;
		}
	}
}

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using UnityEngine;
	using UnityEditor;

	[CustomPropertyDrawer(typeof(FieldLabelAttribute))]
	internal sealed class _FieldLabelAttribute : PropertyDrawer
	{
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			var newLabel = (attribute as FieldLabelAttribute)!.label;
			_tmpLabel.text = newLabel;
			var overrideLabel = string.IsNullOrEmpty(newLabel) ? GUIContent.none : _tmpLabel;
			return EditorGUI.GetPropertyHeight(property, overrideLabel);
		}

		private readonly GUIContent _tmpLabel = new();

		public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent l)
		{
			var newLabel = (attribute as FieldLabelAttribute)!.label;
			_tmpLabel.text = newLabel;
			var label = string.IsNullOrEmpty(newLabel) ? GUIContent.none :_tmpLabel;
			EditorGUI.PropertyField(pos, prop, label);
		}
	}
}