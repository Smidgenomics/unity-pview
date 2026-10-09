// smidgens @ github

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using System;
	using System.Reflection;
	using UnityEngine;

	// misc parse utils
	internal static class PVParse
	{
		public static Color ParseColor(string hexColor, Color dValue)
		{
			// shorthand for empty
			if (hexColor is { Length: 1 } && hexColor[0] == '0')
			{
				return Color.clear;
			}
			return ColorUtility.TryParseHtmlString(hexColor, out var c)
			? c
			: dValue;
		}

		public static float ParseFloat(string str, float defValue)
		{
			return float.TryParse(str.Trim(), out var outVal)
			? outVal
			: defValue;
		}
		
		public static int ParseInt(string str, int defValue)
		{
			return int.TryParse(str.Trim(), out var outVal)
			? outVal
			: defValue;
		}

		public static Rect ParseRect(string str, Rect defValue)
		{
			if (string.IsNullOrEmpty(str))
			{
				return defValue;
			}

			//  atlas position -> uv (index/x cells/y cells) or (index/cells)
			if (str.Contains('/'))
			{
				var segs = str.Split('/');
				if (segs.Length is > 3 or < 2)
				{
					return defValue;
				}
				var i = Mathf.Max(ParseInt(segs[0], 0), 0);
				var cellsX = Mathf.Max(ParseInt(segs[1], 1), 1);
				var cellsY = segs.Length > 2 ? Mathf.Max(ParseInt(segs[2], 1), 1) : cellsX;
				int y = i / cellsX;
				int x = i % cellsY;
				var w = 1f / cellsX;
				var h = 1f / cellsX;
				return new Rect(x * w, y * h, w, h);
			}
			
			var vals = str.Split(',');

			if (vals.Length != 4)
			{
				return defValue;
			}
			Rect outVal = default;
			outVal.x = ParseFloat(vals[0], defValue.x);
			outVal.y = ParseFloat(vals[1], defValue.y);
			outVal.width = ParseFloat(vals[2], defValue.width);
			outVal.height = ParseFloat(vals[3], defValue.height);
			return outVal;
		}

		public static MethodInfo ParseStaticMethodRef(string mRef, Type rType, Type[] pTypes)
		{
			mRef ??= string.Empty;
			int sepIndex = mRef.IndexOf(';');
			if (sepIndex < 0)
			{
				return null;
			}

			var mName = mRef[..sepIndex].Trim();
			var tName = mRef[(sepIndex + 1)..].Trim();

			var type = Type.GetType(tName);
			if (type == null)
			{
				return null;
			}
			const BindingFlags bf = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

			var m = pTypes != null
			? type.GetMethod(mName, bf, null, pTypes, null)
			: type.GetMethod(mName, bf);

			if (m == null || (rType != null && m.ReturnType != rType))
			{
				return null;
			}
			return m;
		}
	}
}