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

		private struct ProfileLoadContext
		{
			// path -> profile
			public Dictionary<string, PVIconProfile> profiles;
			public Queue<string> includeQueue;
		}

		public bool TryGetIconByGUID(string guid, bool small, out LoadedIcon ico)
		{
			ico = default;
			
			if (_guidRules.TryGetValue(guid, out var ip))
			{
				ico = ip.GetIcon(small);
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
					ico = v.GetIcon(small);
					return true;
				}
			}

			if (isFolder && _defaults.folderIcon != null && _defaults.folderIcon.IsValid())
			{
				ico = _defaults.folderIcon.GetIcon(small);
				return true;
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
					foreach (var (key, filterType) in bProfile._keys)
					{
						var isGUID = filterType == typeof(IconFilter_GUID);
						if (!prof._keys.TryAdd(key, filterType))
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

		// profiles to include
		[JsonProperty("includes")] private string[] _includes;
		[JsonProperty("defaults")] private ProfileDefaults _defaults;
		[JsonProperty("extends")] private string _baseProfile { get; set; }
		[JsonProperty("rules")] private Dictionary<string, IconSettings> _rules { get; set; } = new();

		private readonly Dictionary<string,(IconFilter, IconSettings)> _filterRules = new();
		private readonly Dictionary<string, IconSettings> _guidRules = new();
		private readonly Dictionary<string, Type> _keys = new();

		private struct ProfileDefaults
		{
			[JsonProperty] public IconSettings folderIcon;
		}

		public struct LoadedIcon
		{
			public Texture2D tex;
			public Color tint;
			public Rect uv;
			public SkinPick<Color> bgColor;
		}

		[OnDeserialized]
		private void OnDeserialized(StreamingContext ctx)
		{
			foreach (var (r,v) in _rules)
			{
				if (v == null || !v.IsValid())
				{
					continue;
				}
				var f = CreateFilterFromRule(r);
				if (f == null)
				{
					continue;
				}
				_keys[r] = f.GetType();
				if (f is IconFilter_GUID guidRule)
				{
					_guidRules[guidRule.guid] = v;
				}
				else
				{
					_filterRules.Add(r, (f, v));
				}
			}
		}

		private static Texture2D LoadTexture(string guidOrName)
		{
			if (string.IsNullOrEmpty(guidOrName))
			{
				return null;
			}
			if (guidOrName.IsGUID32())
			{
				return AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(guidOrName));
			}

			try
			{
				return EditorGUIUtility.IconContent(guidOrName)?.image as Texture2D;
			}
			catch (Exception e)
			{
				Debug.LogError($"Error parsing icon ref '{guidOrName}': '{e.Message}'");
				return null;
			}
		}

		internal sealed class IconSettings
		{
			[JsonProperty("tex")] private string _iconGUID;
			[JsonProperty("tint")] private string _tint;
			[JsonProperty("uv")] private string _uv; // rect
			[JsonProperty("bg")] private string _bgColor;
			[JsonProperty("sm")] private IconSettings _sm;

			public bool IsValid()
			{
				return loadedIcon.tex;
			}

			public LoadedIcon GetIcon(bool small)
			{
				if (small && _sm?.loadedIcon.tex)
				{
					return _sm.loadedIcon;
				}
				return loadedIcon;
			}

			public void LoadIcon()
			{
				var bgColor = !string.IsNullOrEmpty(_bgColor) && ColorUtility.TryParseHtmlString(_bgColor, out var pColor)
				? new(pColor, pColor)
				: UnityConstants.BrowserColor;

				loadedIcon = new LoadedIcon()
				{
					tint = PVParse.ParseColor(_tint, Color.white),
					uv = PVParse.ParseRect(_uv, new Rect(0f, 0f, 1f, 1f)),
					tex = LoadTexture(_iconGUID),
					bgColor = bgColor
				};
			}
			
			[JsonIgnore] private LoadedIcon loadedIcon { get; set; }
			
			[OnDeserialized]
			private void OnDeserialized(StreamingContext ctx)
			{
				LoadIcon();
			}
		}

		private static IconFilter CreateFilterFromRule(string rule)
		{
			if (rule.IsGUID32())
			{
				return new IconFilter_GUID(rule);
			}

			if (rule.StartsWith("t:"))
			{
				return new IconFilter_Type(rule[2..]);
			}

			var isPattern = rule.Contains('*') || rule.Contains('^');

			// if the path is absolute, we check if a guid already exists for it
			if (!isPattern)
			{
				var path = rule;
				if (path.StartsWith("f:"))
				{
					path = path[2..];
				}
				var possibleGUID = AssetDatabase.AssetPathToGUID(path);
				if (!string.IsNullOrEmpty(possibleGUID))
				{
					return new IconFilter_GUID(possibleGUID);
				}
			}

			if (isPattern)
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
				_rule = typeRule;
				_matchFn = MatchByExactName;

				if (typeRule.StartsWith('^'))
				{
					_regex = new Regex(typeRule);
					_matchFn = MatchByRegex;
				}
				else if (typeRule.Contains('*'))
				{
					_regex = new Wildcard(typeRule);
					_matchFn = MatchByRegex;
				}
				else if(typeRule.Contains(','))
				{
					_exactType = Type.GetType(typeRule);
					if (_exactType != null)
					{
						_matchFn = MatchByExactType;
					}
				}
			}

			public override bool IsMatch(string guid, string path)
			{
				var t = AssetDatabase.GetMainAssetTypeAtPath(path);
				return t != null && _matchFn.Invoke(t);
			}

			private readonly string _rule;
			private readonly Type _exactType;
			private readonly Regex _regex;
			private readonly Func<Type, bool> _matchFn;

			private bool MatchByExactType(Type t) => t == _exactType;
			private bool MatchByRegex(Type t) => _regex.IsMatch(t.AssemblyQualifiedName!);
			private bool MatchByExactName(Type t) => t.Name == _rule;

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
			public string guid { get; }
			
			public IconFilter_GUID(string guid)
			{
				this.guid = guid;
			}

			public override bool IsMatch(string inGUID, string path) => guid == inGUID;
			
		}
		
		
	}
}