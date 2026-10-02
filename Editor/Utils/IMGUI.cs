// smidgens @ github

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using UnityEngine;
	using System;
	using UnityEditor;

	internal static class IMGUI
	{
		public static void DrawSprite
		(
			in Rect area,
			Texture tex,
			in Rect coords,
			in Color color
		)
		{
			var tc = GUI.color;
			GUI.color = color;

			using (new GUI.ClipScope(area))
			{
				var sx = 1f / coords.size.x;
				var sy = 1f / coords.size.y;
				var ir = area;
				ir.size = new Vector2
				(
					sx * area.width,
					sy * area.width
				);
				ir.position = new Vector2
				(
					-coords.position.x * area.width * sx,
					-coords.position.y * area.height * sy
				);
				GUI.DrawTexture(ir, tex, ScaleMode.StretchToFill);
			}
			GUI.color = tc;
		}
	}
}
