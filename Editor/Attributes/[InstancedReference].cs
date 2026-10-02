// smidgens @ github

// resharper disable all

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using System;
	using System.Diagnostics;
	using UnityEngine;

	[AttributeUsage(AttributeTargets.Field)]
	[Conditional("UNITY_EDITOR")]
	internal sealed class InstancedReferenceAttribute : PropertyAttribute
	{
		public bool indent { get; }
		public string defaultValueLabel { get; set; } = "(none)";

		public InstancedReferenceAttribute(string defaultLabel = null, bool indent = true)
		{
			this.indent = indent;
		}
	}
}

#if UNITY_EDITOR

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using UnityEngine;
	using UnityEditor;
	using System;
	using System.Collections.Generic;
	using System.Reflection;
	using System.ComponentModel;
	using UnityEditor.Search;
	using UObject = UnityEngine.Object;

	[CustomPropertyDrawer(typeof(InstancedReferenceAttribute))]
	internal sealed class _InstancedReferenceAttribute : PropertyDrawer
	{
		public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent l)
		{
			if (prop.propertyType != SerializedPropertyType.ManagedReference)
			{
				pos = EditorGUI.PrefixLabel(pos, l);
				EditorGUI.LabelField(pos, "Invalid type", EditorStyles.miniLabel);
				return;
			}

			var typeRect = pos.SliceTop(EditorGUIUtility.singleLineHeight);
			pos.SliceTop(2);

			if(l != GUIContent.none && !fieldInfo.FieldType.IsArray)
			{
				typeRect = EditorGUI.PrefixLabel(typeRect, l);
			}

			using (new EditorGUI.PropertyScope(pos, l, prop))
			{
				SelectorDropdown(typeRect, prop);
				if (prop.managedReferenceValue == null)
				{
					return;
				}

				var attr = (attribute as InstancedReferenceAttribute)!;
	
				var extraIndent = 1;

				if (!attr.indent)
				{
					extraIndent = 0;
				}
		
				EditorGUI.indentLevel += extraIndent;
				foreach (var field in _cachedFields.Item2)
				{
					var fProp = prop.serializedObject.FindProperty(prop.propertyPath + "." + field.Name);
					var propHeight = EditorGUI.GetPropertyHeight(fProp);
					var fRect = pos.SliceTop(propHeight);
					EditorGUI.PropertyField(fRect, fProp);
					pos.SliceTop(2);
				}
				EditorGUI.indentLevel -= extraIndent;
			}
		}

		public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
		{
			if (_cachedFields.Item1 != prop.managedReferenceValue?.GetType())
			{
				var currentType = prop.managedReferenceValue?.GetType();
				var fields = currentType != null ? currentType.FindInspectorFields<object>() : null;
				_cachedFields = (currentType, fields);
			}

			int rowCount = 1;

			if (prop.managedReferenceValue != null)
			{
				rowCount += _cachedFields.Item2.Count;
			}
			
			var padding = (rowCount - 1) * 2;

			float fieldHeight = 0f;

			if (_cachedFields.Item2 != null)
			{
				foreach (var f in _cachedFields.Item2)
				{
					var fProp = prop.serializedObject.FindProperty(prop.propertyPath + "." + f.Name);
					fieldHeight += EditorGUI.GetPropertyHeight(fProp);
				}
			}

			return (rowCount) * EditorGUIUtility.singleLineHeight + padding + fieldHeight;
		}

		private GUIContent _btnLabel = new();
		private (Type, IReadOnlyList<FieldInfo>) _cachedFields;

		private void SelectorDropdown(Rect pos, SerializedProperty prop)
		{
			Type currentType = prop.managedReferenceValue?.GetType();

			var defLabel = (attribute as InstancedReferenceAttribute).defaultValueLabel;

			var btnLabel = currentType != null
			? currentType.Name
			: defLabel;

			var dn = currentType?.GetCustomAttribute<DisplayNameAttribute>();
			if (dn != null)
			{
				btnLabel = dn.DisplayName;
			}
			
			_btnLabel.text = btnLabel;

			if (!EditorGUI.DropdownButton(pos, _btnLabel, FocusType.Keyboard))
			{
				return;
			}

			var menu = CreateTypeDropdown(GetFieldType(), currentType,  newType =>
			{
				if (newType == currentType)
				{
					return;
				}

				if (newType == null)
				{
					prop.managedReferenceValue = null;
					prop.serializedObject.ApplyModifiedProperties();
					return;
				}
				
				prop.managedReferenceValue = Activator.CreateInstance(newType);
				prop.serializedObject.ApplyModifiedProperties();
			}, defLabel);
		
			menu.DropDown(pos);
			
		}

		private Type GetFieldType()
		{
			return !fieldInfo.FieldType.IsArray
			? fieldInfo.FieldType
			: fieldInfo.FieldType.GetElementType();
		}
		
		public static GenericMenu CreateTypeDropdown(Type baseType, Type currentType, Action<Type> fn, string defaultLabel = "(none)")
		{
			// var dropdown = GenericDropdown<Type>.Create(ObjectNames.NicifyVariableName(baseType.Name));
			var menu = new GenericMenu();

			var types = TypeCache.GetTypesDerivedFrom(baseType);

			if (!string.IsNullOrEmpty(defaultLabel))
			{
				// dropdown.AddSeparator(string.Empty);
			}

			foreach (var type in types)
			{
				if (type.IsDefined(typeof(ObsoleteAttribute)))
				{
					continue;
				}

				
				var tVal = type;
				var dname = GetTypeLabel(type);
				// var icon = SearchUtils.GetTypeIcon(type);
				
				if (type == currentType)
				{
					menu.AddDisabledItem(dname, false);
					continue;
				}
				
				menu.AddItem(dname, false, () =>
				{
					fn.Invoke(tVal);
				});
			}
			return menu;
		}

		private static GUIContent GetTypeLabel(Type type)
		{
			string category = null;
			string dname = null;

			var md = type.GetCustomAttribute<DisplayNameAttribute>();

			if (md != null)
			{
				// category = md.category;
				dname = md.DisplayName;
			}
			if (dname == null)
			{
				dname = type.Name;
			}
			var path = category != null ? category + "/" + dname : dname;
			return new GUIContent(path);
		}
	}
}

#endif