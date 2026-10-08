// smidgens @ github

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using System;
	using System.Reflection;
	using UnityEngine;

	// misc parse utils
	internal static class PVParse
	{
		public static Color ParseHexColor(string hexColor, Color dValue)
		{
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

		public static Rect ParseRect(string str, Rect defValue)
		{
			if (string.IsNullOrEmpty(str))
			{
				return defValue;
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