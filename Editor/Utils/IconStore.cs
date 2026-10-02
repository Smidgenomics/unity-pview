// smidgens @ github

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using System;
	using System.Collections.Generic;
	using UnityEditor;
	using UnityEngine;

	// key/value store for icons using guids or aliases
	internal sealed class IconStore
	{
		public Texture2D GetIconOrCached(string guidOrAlias)
		{
			if (guidOrAlias == null)
			{
				return null;
			}
			if (!_cachedIcons.TryGetValue(guidOrAlias, out var tex))
			{
				tex = LoadTex(guidOrAlias);
				_cachedIcons.Add(guidOrAlias, tex);
			}
			return null;
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