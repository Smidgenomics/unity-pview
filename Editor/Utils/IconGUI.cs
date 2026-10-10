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

		
		public static void DrawIcon(Rect rect, bool adjust, Texture icon, in Rect uvCoords, Color bgColor, Color tint = default)
		{
			if (adjust)
			{
				TweakLayoutSize(ref rect);
			}
			EditorGUI.DrawRect(rect, bgColor);
			if (Mathf.Approximately(tint.a, 0f))
			{
				tint = Color.white;
			}
			IMGUI.DrawSprite(rect, icon, uvCoords, tint);
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