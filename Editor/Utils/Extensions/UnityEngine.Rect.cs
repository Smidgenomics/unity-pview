// smidgens @ github

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using UnityEngine;

	internal static class Rect_
	{
		public static Rect SliceTop(this ref Rect r, in float s)
		{
			var r2 = r;
			r2.height = s;
			r.height -= s;
			r.position += new Vector2(0f, s);
			return r2;
		}

		public static Rect SliceBottom(this ref Rect r, in float s)
		{
			var r2 = r;
			r2.height = s;
			r.height -= s;
			r2.y += r.height;
			return r2;
		}

		public static Rect SliceLeft(this ref Rect r, in float w)
		{
			var r2 = r;
			r2.width = w;
			r.width -= w;
			r.x += w;
			return r2;
		}

		public static Rect SliceRight(this ref Rect r, in float w)
		{
			var r2 = r;
			r2.width = w;
			r.width -= w;
			r2.x += r.width;
			return r2;
		}

		public static void Resize(this ref Rect r, in float s) => r.Resize(s, s, s, s);
		public static void Resize(this ref Rect r, float h, in float v) => r.Resize(h, h, v, v);

		public static void Resize
		(
			this ref Rect rect,
			in float l,
			in float r,
			in float t,
			in float b
		)
		{
			var c = rect.center;
			rect.width += l + r;
			rect.height += t + b;
			rect.center = c;
		}
	}
}