// smidgens @ github

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using System;
	using System.Collections.Generic;
	using System.Runtime.Serialization;
	using Newtonsoft.Json;
	using UnityEditor;
	using UnityEngine;

	/// <summary>
	/// Icon JSON file
	/// </summary>
	[Serializable]
	internal sealed class PVIconProfile
	{
		public static PVIconProfile FromJSON(string data)
		{
			return JsonConvert.DeserializeObject<PVIconProfile>(data, GetSerializationSettings());
		}

		private static JsonSerializerSettings GetSerializationSettings()
		{
			_cachedSerializationSettings ??= new JsonSerializerSettings
			{
				TypeNameHandling = TypeNameHandling.All,
				MissingMemberHandling = MissingMemberHandling.Ignore,
			};
			return _cachedSerializationSettings;
		}

		private static JsonSerializerSettings _cachedSerializationSettings;
		
		
		public bool TryGetIconByGUID(string guid, out LoadedIcon ico)
		{
			ico = default;

			if (_guidRules.TryGetValue(guid, out var ip))
			{
				ico = ip.loadedIcon;
				return true;
			}

			var path = PathFromGUID(guid);
			var isFolder = AssetDatabase.IsValidFolder(path);
			foreach (var (f, v) in _rules)
			{
				if (isFolder && !f.IsFolder())
				{
					continue;
				}
				if (f.IsMatch(guid, path))
				{
					ico = v.loadedIcon;
					return true;
				}
			}
			return false;
		}

		[JsonProperty] internal Dictionary<string, IconPrefs> rules { get; private set; } = new();

		private readonly List<(IconFilter, IconPrefs)> _rules = new();
		private readonly Dictionary<string, IconPrefs> _guidRules = new();

		public struct LoadedIcon
		{
			public Color tint;
			public Rect coords;
			public Texture2D icon;
		}

		[OnDeserialized]
		private void OnDeserialized(StreamingContext ctx)
		{
			foreach (var (r,v) in rules)
			{
				if (v == null || !v.loadedIcon.icon)
				{
					continue;
				}
				var f = CreateFilterFromRule(r);
				if (f == null)
				{
					continue;
				}

				if (f is IconFilter_GUID)
				{
					_guidRules[r] = v;
				}
				else
				{
					_rules.Add((f, v));
				}
			}
		}

		private static Color ParseHexColor(string str, Color defValue)
		{
			return ColorUtility.TryParseHtmlString(str, out var outColor)
			? outColor
			: defValue;
		}

		private static float ParseFloat(string str, float defValue)
		{
			return float.TryParse(str.Trim(), out var outVal)
			? outVal
			: defValue;
		}

		private static Rect ParseRect(string str, Rect defValue)
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
			outVal.x = ParseFloat(vals[1], defValue.y);
			outVal.width = ParseFloat(vals[2], defValue.width);
			outVal.height = ParseFloat(vals[3], defValue.height);
			return outVal;
		}

		private static Texture2D LoadTexture(string guidOrName)
		{
			if (guidOrName.IsGUID32())
			{
				return AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(guidOrName));
			}
			return EditorGUIUtility.IconContent(guidOrName)?.image as Texture2D;
		}
		
		internal sealed class IconPrefs
		{
			[JsonProperty("icon")] private string _iconGUID;
			[JsonProperty("tint")] private string _tint;
			[JsonProperty("coords")] private string _coords; // rect
			[JsonIgnore] public LoadedIcon loadedIcon { get; private set; }

			[OnDeserialized]
			private void OnDeserialized(StreamingContext ctx)
			{
				loadedIcon = new LoadedIcon()
				{
					tint = ParseHexColor(_tint, Color.white),
					coords = ParseRect(_coords, new Rect(0f, 0f, 1f, 1f)),
					icon = LoadTexture(_iconGUID),
				};
			}
		}

		private static bool IsPathString(string rule)
		{
			return rule.Contains('/') || rule.Contains('*');
		}

		private static IconFilter CreateFilterFromRule(string rule)
		{
			if (rule.IsGUID32())
			{
				return new IconFilter_GUID(rule);
			}

			if (rule.StartsWith("t:"))
			{
				return new IconFilter_Type(rule);
			}

			if (IsPathString(rule))
			{
				return new IconFilter_Path(rule);
			}
			return null;
		}
		
		private static string PathFromGUID(string guid)
		{
			return AssetDatabase.GUIDToAssetPath(guid);
		}

		internal abstract class IconFilter
		{
			public virtual bool IsMatch(string guid, string path) => false;
			public virtual bool IsFolder() => false;
		}

		internal sealed class IconFilter_Type : IconFilter
		{
			public IconFilter_Type(string typeRule)
			{
				typeRule = typeRule.Replace("t:", "");
				_explicitType = Type.GetType(typeRule);
				_wildcard = new Wildcard(typeRule);
			}

			public override bool IsMatch(string guid, string path)
			{
				var t = AssetDatabase.GetMainAssetTypeAtPath(path);
				if (t == null)
				{
					return false;
				}

				if (_explicitType != null)
				{
					return _explicitType.IsAssignableFrom(t);
				}
				return _wildcard.IsMatch(t.Name);
			}

			private readonly Type _explicitType;
			private readonly Wildcard _wildcard;

		}

		internal sealed class IconFilter_Path : IconFilter
		{
			public IconFilter_Path(string wildcard)
			{
				if (wildcard.StartsWith("f:"))
				{
					_folder = true;
					wildcard = wildcard.Substring(2);
				}
				_wildcard = new Wildcard(wildcard);
			}
			public override bool IsFolder() => _folder;

			public override bool IsMatch(string guid, string path)
			{
				return _wildcard.IsMatch(path);
			}

			private readonly bool _folder;
			private readonly Wildcard _wildcard;
		}

		internal sealed class IconFilter_GUID : IconFilter
		{
			public IconFilter_GUID(string guid)
			{
				this.guid = guid;
			}

			public override bool IsMatch(string inGUID, string path) => this.guid == inGUID;
			
			private readonly string guid;
		}
		
		
	}
}