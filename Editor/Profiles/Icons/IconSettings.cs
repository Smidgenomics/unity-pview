// smidgens @ github

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using Newtonsoft.Json;
	using UnityEngine;

	internal sealed class IconSettings
	{
		public IconSettings smallVariant => _sm;

		public LoadedIcon LoadIcon()
		{
			var bgColor = !string.IsNullOrEmpty(_bgColor) && PVParse.TryParseColor(_bgColor, out var pColor)
			? new(pColor, pColor)
			: UnityConstants.BrowserColor;

			var pos = PVParse.ParseRect(_pos, new Rect(0, 0, 1f, 1f));
			return new LoadedIcon()
			{
				tint = PVParse.ParseColor(_tint, Color.white),
				uv = PVParse.ParseRect(_uv, new Rect(0f, 0f, 1f, 1f)),
				tex = PVUtils.LoadTexture(_iconGUID),
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