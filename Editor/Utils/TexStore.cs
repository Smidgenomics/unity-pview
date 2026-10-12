// smidgens @ github

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using System;
	using System.Collections.Generic;
	using UnityEditor;
	using UnityEngine;

	// key/value store for icons using guids or aliases
	internal sealed class TexStore
	{
		public Texture2D GetIconOrCached(string guidOrAlias, Texture2D dValue = null)
		{
			if (string.IsNullOrEmpty(guidOrAlias))
			{
				return dValue;
			}
			if (!_cachedIcons.TryGetValue(guidOrAlias, out var tex))
			{
				tex = LoadTex(guidOrAlias);
				_cachedIcons.Add(guidOrAlias, tex);
			}
			return tex;
		}

		private readonly Dictionary<string, Texture2D> _cachedIcons = new();

		private Texture2D LoadTex(string guidOrAlias)
		{
			// asset guid
			if (guidOrAlias.IsGUID32())
			{
				var path = AssetDatabase.GUIDToAssetPath(guidOrAlias);
				return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
			}
			return EditorGUIUtility.IconContent(guidOrAlias)?.image as Texture2D;
		}
	}

}