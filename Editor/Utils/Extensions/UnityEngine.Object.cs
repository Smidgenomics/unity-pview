// smidgens @ github

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using System;
	using System.Collections.Generic;
	using UnityEditor;
	using UnityEngine;
	using UObject = UnityEngine.Object;

	/// <summary>
	/// Extensions for UnityEngine.Object
	/// </summary>
	internal static class UnityObject_
	{
		/// <summary>
		/// Checks if object is asset (exists on disk)
		/// </summary>
		public static bool IsAsset(this UObject o) => o && EditorUtility.IsPersistent(o);

		/// <summary>
		/// Instantiates nested asset
		/// </summary>
		public static UObject InstantiateNestedSO(this UObject parent, Type type, string name = null, bool hide = true)
		{
			if (!parent.IsAsset())
			{
				return null;
			}
			var ob = ScriptableObject.CreateInstance(type);
			if (name == null)
			{
				name = $"New {type.Name}";
			}
			ob.name = name;
			// hidden from user in project
			if (hide)
			{
				ob.hideFlags = HideFlags.HideInHierarchy;
			}
			Undo.RegisterCreatedObjectUndo(ob, $"Instantiate {type.Name}");
			AssetDatabase.AddObjectToAsset(ob, parent);
			return ob;
		}
		
		/// <summary>
		/// Loads all nested assets of given type
		/// </summary>
		public static T[] LoadNestedAssets<T>(this UObject target)
		{
			if (!target.IsAsset())
			{
				return Array.Empty<T>();
			}

			List<T> assets = new();
			var path = AssetDatabase.GetAssetPath(target);

			foreach (var a in AssetDatabase.LoadAllAssetsAtPath(path))
			{
				if (a is T validAsset)
				{
					assets.Add(validAsset);
				}
			}
			return assets.ToArray();
		}
	}
}
