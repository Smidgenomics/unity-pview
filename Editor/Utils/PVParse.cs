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
			return TryParseColor(hexColor, out var c)
			? c
			: dValue;
		}

		public static SkinPick<T> ParseSkinValue<T>(string str, Func<string, T, T> fn, T dValue)
		{
			if (string.IsNullOrEmpty(str))
			{
				return dValue;
			}
			// return fn.Invoke(str, dValue);
			if (str.Contains(';'))
			{
				var segs = str.Split(';');
				if (segs.Length == 2)
				{
					var v1 = fn.Invoke(segs[0], dValue);
					var v2 = fn.Invoke(segs[1], dValue);
					return new(v1, v2);
				}
			}
			return fn.Invoke(str, dValue);
		}

		public static bool TryParseColor(string hexColor, out Color outColor)
		{
			if (string.IsNullOrEmpty(hexColor))
			{
				outColor = default;
				return false;
			}

			// shorthand for empty
			if (hexColor is "0" or "clear")
			{
				outColor = Color.clear;
				return true;
			}

			// quick hack to allow #fff or #ffff shorthands which TryParseHtmlString seems to choke on
			// note: will simply replace TryParseHtmlString with hex parsing when I cba
			if (hexColor[0] == '#' && hexColor.Length is 4 or 5)
			{
				var newStr = "#";
				foreach (var c in hexColor[1..])
				{
					newStr += $"{c}{c}";
				}
				hexColor = newStr;
			}
			return ColorUtility.TryParseHtmlString(hexColor, out outColor);
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

		public static int ParseHex(string str)
		{
			// handy approach courtesy of:
			// https://stackoverflow.com/questions/74148/how-to-convert-numbers-between-hexadecimal-and-decimal/9126750#9126750
			if (string.IsNullOrEmpty(str))
			{
				return 0;
			}
			int decValue = _HEX_TO_DEC[(byte)str[0]];
			for (var i = 1; i < str.Length; i++)
			{
				decValue *= 16;
				decValue += _HEX_TO_DEC[(byte)str[i]];
			}
			return decValue;
		}

		private static readonly sbyte[] _HEX_TO_DEC =
		{
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 0, 1, 2, 3, 4, 5, 6, 7,
			8, 9, -1, -1, -1, -1, -1, -1, -1, 10, 11, 12, 13, 14, 15, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 10, 11, 12, 13, 14, 15, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1
		};
	}
}