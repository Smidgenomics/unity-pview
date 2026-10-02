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

		public OptionalValue(T value)
		{
			this.value = default;
			enabled = true;
		}
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
	using System.Reflection;
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
			var lWidthAttr = fieldInfo.GetCustomAttribute<FieldLabelWidthAttribute>();
			var eProp = prop.FindPropertyRelative(nameof(OptionalValue<byte>.enabled));
			var vProp = prop.FindPropertyRelative(nameof(OptionalValue<byte>.value));
			var lWidth = lWidthAttr?.width ?? 	EditorGUIUtility.labelWidth;
			var toggleRect = position.SliceLeft(EditorGUIUtility.singleLineHeight);
			position.SliceLeft(EditorGUIUtility.standardVerticalSpacing);
			var lRect = position.SliceLeft(lWidth);
			position.SliceLeft(EditorGUIUtility.standardVerticalSpacing);
			EditorGUI.PropertyField(toggleRect, eProp, GUIContent.none);
			var tEnabled = GUI.enabled;
			GUI.enabled = eProp.boolValue;
			EditorGUI.LabelField(lRect, label);
			EditorGUI.indentLevel--;
			EditorGUI.PropertyField(position, vProp, GUIContent.none);
			EditorGUI.indentLevel++;
			GUI.enabled = tEnabled;
		}
	}
	
}