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
		public static bool IsSmallView(in Rect rect)
		{
			return rect.width > rect.height;
		}

		public static void DrawIcon(Rect rect, bool adjust, in LoadedIcon icon)
		{
			if (adjust)
			{
				TweakLayoutSize(ref rect);
			}
			EditorGUI.DrawRect(rect, icon.bgColor);
			var tint = icon.tint;
			if (Mathf.Approximately(tint.a, 0f))
			{
				tint = Color.white;
			}

			var pos = icon.pos;
			var ox = rect.width * pos.x;
			var oy = rect.height * pos.y;

			rect.position += new Vector2(ox, oy);
			rect.width *= pos.width;
			rect.height *= pos.height;

			IMGUI.DrawSprite(rect, icon.tex, icon.uv, tint);
		}

		private static void TweakLayoutSize(ref Rect rect)
		{
			var isSmall = rect.width > rect.height;
			if (isSmall)
			{
				rect.width = rect.height;
			}
			else
			{
				rect.height = rect.width;
			}
			const float lgSize = UnityConstants.BROWSER_ICON_LG;
			if (rect.width > lgSize)
			{
				var offset = (rect.width - lgSize) / 2f;
				rect = new Rect(rect.x + offset, rect.y + offset, lgSize, lgSize);
				return;
			}

			if (isSmall && !UnityUtility.IsProjectTreeViewItem(rect))
			{
				rect = new Rect(rect.x + 3f, rect.y, rect.width, rect.height);
			}
		}
	}
}