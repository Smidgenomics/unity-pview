// smidgens @ github

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using System;
	using Newtonsoft.Json;
	using UnityEngine;

	internal sealed class IconSettings
	{
		public IconSettings smallVariant => _sm;

		public LoadedIcon LoadIcon(TexStore texStore)
		{
			var bgColor = string.IsNullOrEmpty(_bgColor)
			? UnityConstants.BrowserColor
			: PVParse.ParseSkinValue(_bgColor, PVParse.ParseColor, Color.clear);

			var pos = PVParse.ParseSkinValue(_pos, PVParse.ParseRect, new Rect(0, 0, 1f, 1f));
			var uv = PVParse.ParseSkinValue(_uv, PVParse.ParseRect, new Rect(0, 0, 1f, 1f));
			var tint = PVParse.ParseSkinValue(_tint, PVParse.ParseColor, Color.white);
			var tex = PVParse.ParseSkinValue<Texture2D>(_iconGUID, texStore.GetIconOrCached, null);
			return new LoadedIcon()
			{
				tint = tint,
				uv = uv,
				tex = tex,
				bgColor = bgColor,
				floatRight = _right,
				pos = pos,
			};
		}

		[JsonProperty("tex")] private string _iconGUID;
		[JsonProperty("tint")] private string _tint;
		[JsonProperty("uv")] private string _uv; // rect
		[JsonProperty("pos")] private string _pos; // rect
		[JsonProperty("bg")] private string _bgColor;
		[JsonProperty("sm")] private IconSettings _sm;
		[JsonProperty("right")] private bool _right;
	}
}