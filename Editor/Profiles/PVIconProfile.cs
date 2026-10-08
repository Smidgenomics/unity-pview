// smidgens @ github

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Runtime.Serialization;
	using System.Text.RegularExpressions;
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
			return FromJSON(data, true);
		}

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
			foreach (var (k, val) in _filterRules)
			{
				var (f, v) = val;
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

		private static PVIconProfile FromJSON(string data, bool recursive)
		{
			var prof = JsonConvert.DeserializeObject<PVIconProfile>(data, GetSerializationSettings());

			if (recursive && !string.IsNullOrEmpty(prof._baseProfile))
			{
				var baseData = ReadRelativeFile(prof._baseProfile);

				if (baseData == null)
				{
					return prof;
				}
				
				var bProfile = FromJSON(baseData, false);

				if (bProfile._rules != null)
				{
					foreach (var (key, isGUID) in bProfile._keys)
					{
						if (!prof._keys.TryAdd(key, isGUID))
						{
							continue;
						}
						if (isGUID)
						{
							prof._guidRules.Add(key, bProfile._guidRules[key]);
						}
						else
						{
							prof._filterRules.Add(key, bProfile._filterRules[key]);
						}
						
					}
				}
			}
			return prof;
		}

		private static string ReadRelativeFile(string pPath)
		{
			var fPath = PVConstants.PROJECT_ROOT + "/" + pPath;
			if (File.Exists(fPath))
			{
				return File.ReadAllText(fPath);
			}
			return null;
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

		[JsonProperty("extends")] private string _baseProfile { get; set; }
		[JsonProperty("rules")] private Dictionary<string, IconPrefs> _rules { get; set; } = new();

		private readonly Dictionary<string,(IconFilter, IconPrefs)> _filterRules = new();
		private readonly Dictionary<string, IconPrefs> _guidRules = new();
		private readonly Dictionary<string, bool> _keys = new();

		public struct LoadedIcon
		{
			public Color tint;
			public Rect coords;
			public Texture2D icon;
		}

		[OnDeserialized]
		private void OnDeserialized(StreamingContext ctx)
		{
			foreach (var (r,v) in _rules)
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
					_keys[r] = true;
					_guidRules[r] = v;
				}
				else
				{
					_keys[r] = false;
					_filterRules.Add(r, (f, v));
				}
			}
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
					tint = PVParse.ParseHexColor(_tint, Color.white),
					coords = PVParse.ParseRect(_coords, new Rect(0f, 0f, 1f, 1f)),
					icon = LoadTexture(_iconGUID),
				};
			}
		}

		private static bool IsPathString(string rule)
		{
			return rule.Contains('/') || rule.Contains('*') || rule.StartsWith('^');
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
			public IconFilter_Path(string pattern)
			{
				if (pattern.StartsWith("f:"))
				{
					_folder = true;
					pattern = pattern.Substring(2);
				}
				_regex = pattern.StartsWith('^') && pattern.EndsWith('$')
				? new Regex(pattern)
				: new Wildcard(pattern);
			}
			public override bool IsFolder() => _folder;

			public override bool IsMatch(string guid, string path)
			{
				return _regex.IsMatch(path);
			}

			private readonly bool _folder;
			private readonly Regex _regex;
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