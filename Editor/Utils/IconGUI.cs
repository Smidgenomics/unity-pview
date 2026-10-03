// smidgens @ github

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using UnityEngine;
	using UnityEditor;

	/// <summary>
	/// Methods for drawing icons in project view
	/// </summary>
	internal static class IconGUI
	{
		private static readonly SkinPick<Color> _BACKGROUND_COLOR = new
		(
			ParseColor(UnityConstants.COLOR_BROWSER_BG_L),
			ParseColor(UnityConstants.COLOR_BROWSER_BG_D)
		);

		private const float _PV_ICON_LG = 88f;

		private static Color ParseColor(string hexColor)
		{
			return ColorUtility.TryParseHtmlString(hexColor, out var c)
			? c
			: default;
		}

		public static void DrawIcon
		(
			Rect rect, Texture icon,
			in Rect uvCoords,
			Color tint = default
		)
		{
			var isSmall = IsSmallish(rect);
			TweakLayoutSize(ref rect, isSmall);
			DrawBrowserColor(rect);
			if (Mathf.Approximately(tint.a, 0f))
			{
				tint = Color.white;
			}
			Draw(rect, icon, uvCoords, tint);
		}

		private static void TweakLayoutSize(ref Rect rect, in bool small)
		{
			EqualizeRatio(ref rect, small);

			if (rect.width > _PV_ICON_LG)
			{
				var offset = (rect.width - _PV_ICON_LG) / 2f;
				rect = new Rect(rect.x + offset, rect.y + offset, _PV_ICON_LG, _PV_ICON_LG);
				return;
			}

			if (small && !IsTreeViewPanel(rect))
			{
				rect = new Rect(rect.x + 3f, rect.y, rect.width, rect.height);
			}
		}

		// should rect be considered small
		private static bool IsSmallish(in Rect rect) => rect.width > rect.height;

		// scaling
		private static void EqualizeRatio(ref Rect rect, in bool small)
		{
			if (small)
			{
				rect.width = rect.height;
			}
			else
			{
				rect.height = rect.width;
			}
		}

		private static void Draw(in Rect r, Texture tex, in Rect coords, Color tint)
		{
			if (!tex)
			{
				return;
			}
			IMGUI.DrawSprite(r, tex, coords, tint);
		}

		// draw project view background color
		private static void DrawBrowserColor(in Rect r) => EditorGUI.DrawRect(r, _BACKGROUND_COLOR);

		// hack to check if we're inside p.browser tree view
		private static bool IsTreeViewPanel(in Rect rect) => (rect.x - 16) % 14 == 0;
	}
}