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

		// load texture by guid, name (internal editor icon), or asset path
		public static Texture2D LoadTexture(string guidOrNameOrPath)
		{
			if (string.IsNullOrEmpty(guidOrNameOrPath))
			{
				return null;
			}
			if (guidOrNameOrPath.IsGUID32())
			{
				return AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(guidOrNameOrPath));
			}
			
			// Assets/ or Packages/ supported
			if (guidOrNameOrPath.Contains('/'))
			{
				return RelativePathExists(guidOrNameOrPath)
				? AssetDatabase.LoadAssetAtPath<Texture2D>(guidOrNameOrPath)
				: null;
			}

			try
			{
				return EditorGUIUtility.IconContent(guidOrNameOrPath)?.image as Texture2D;
			}
			catch (Exception e)
			{
				Debug.LogError($"Error parsing icon ref '{guidOrNameOrPath}': '{e.Message}'");
				return null;
			}
		}

		public static bool RelativePathExists(string path)
		{
			var absPath = $"{PVConstants.PROJECT_ROOT}/{path}";
			return File.Exists(absPath);
		}
	}
}