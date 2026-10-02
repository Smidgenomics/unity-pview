// smidgens @ github

#if UNITY_EDITOR

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using System;
	using UnityEngine;

	/// <summary>
	/// Turns a string field into an asset field, serialized as an asset GUID
	/// </summary>
	[AttributeUsage(AttributeTargets.Field)]
	internal sealed class AssetGUIDAttribute : PropertyAttribute
	{
		internal Type baseType { get; }

		public AssetGUIDAttribute(Type baseType = null)
		{
			baseType ??= typeof(UnityEngine.Object);
			if (typeof(UnityEngine.Object).IsAssignableFrom(baseType))
			{
				this.baseType = baseType;
			}
		}
	}
}

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using UnityEngine;
	using UnityEditor;

	using UnityObject = UnityEngine.Object;
	using SP = UnityEditor.SerializedProperty;
	using Type = System.Type;

	[CustomPropertyDrawer(typeof(AssetGUIDAttribute))]
	internal sealed class _AssetGUIDAttribute : PropertyDrawer
	{
		public override void OnGUI(Rect pos, SP prop, GUIContent l)
		{
			// label not blank and item not inside array
			if (l != GUIContent.none && !fieldInfo.FieldType.IsArray)
			{
				pos = EditorGUI.PrefixLabel(pos, l);
			}

			if(prop.propertyType != SerializedPropertyType.String)
			{
				MutedError(pos, "Invalid type");
				return;
			}


			using (new EditorGUI.PropertyScope(pos, l, prop))
			{
				var currentRef = FromGUID(prop.stringValue);
				
				var newRef = EditorGUI.ObjectField(pos, currentRef, GetSearchType(), false);
				if(currentRef != newRef)
				{
					prop.stringValue = GetGUID(newRef);
				}
			}
		}

		private Type GetSearchType()
		{
			AssetGUIDAttribute a = attribute as AssetGUIDAttribute;
			return a.baseType ?? typeof(UnityObject);
		}

		private static UnityObject FromGUID(in string guid)
		{
			if (string.IsNullOrEmpty(guid)) { return null; }
			var path = AssetDatabase.GUIDToAssetPath(guid);
			return AssetDatabase.LoadAssetAtPath<UnityObject>(path);
		}

		private static string GetGUID(UnityObject ob)
		{
			if (!ob) { return ""; }
			var path = AssetDatabase.GetAssetPath(ob);
			return AssetDatabase.AssetPathToGUID(path);
		}

		private static void MutedError(in Rect pos, in string msg) => MutedInfo(pos, msg, Color.red);

		private static void MutedInfo(in Rect pos, in string msg, in Color c)
		{
			EditorGUI.DrawRect(pos, c * 0.3f);
			EditorGUI.LabelField(pos, msg, EditorStyles.centeredGreyMiniLabel);
		}

	}
}

#endif