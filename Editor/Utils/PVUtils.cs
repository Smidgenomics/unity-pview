// smidgens @ github

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using System;
	using System.IO;
	using UnityEditor;
	using UnityEngine;

	internal static class PVUtils
	{
		public static string ReadRelativeFile(string pPath)
		{
			var fPath = $"{PVConstants.PROJECT_ROOT}/{pPath}";
			return File.Exists(fPath) ? File.ReadAllText(fPath) : null;
		}

		public static Texture2D LoadTexture(string guidOrName)
		{
			if (string.IsNullOrEmpty(guidOrName))
			{
				return null;
			}
			if (guidOrName.IsGUID32())
			{
				return AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(guidOrName));
			}

			try
			{
				return EditorGUIUtility.IconContent(guidOrName)?.image as Texture2D;
			}
			catch (Exception e)
			{
				Debug.LogError($"Error parsing icon ref '{guidOrName}': '{e.Message}'");
				return null;
			}
		}
	}
}