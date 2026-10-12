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
				tex = PVUtils.LoadTexture(guidOrAlias);
				_cachedIcons.Add(guidOrAlias, tex);
			}
			return tex;
		}

		private readonly Dictionary<string, Texture2D> _cachedIcons = new();
	}

}