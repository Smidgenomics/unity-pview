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

		private struct MultiIcon
		{
			public MultiIcon(SkinPick<Texture2D> icon, SkinPick<Texture2D> iconSM)
			{
				_icon = icon;
				_iconSM = iconSM;
			}
			private SkinPick<Texture2D> _icon;
			private SkinPick<Texture2D> _iconSM;
		}

		public struct LoadedIcon
		{
			public Color tint;
			public Rect coords;
			public SkinPick<Color> bgColor;
			public Texture2D icon;
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

		internal sealed class IconPrefs
		{
			[JsonProperty("tex")] private string _iconGUID;
			[JsonProperty("tint")] private string _tint;
			[JsonProperty("uv")] private string _uv; // rect
			[JsonProperty("bg")] private string _bgColor;
			[JsonProperty("sm")] private IconPrefs _sm;

			public bool IsValid()
			{
				return loadedIcon.icon;
			}

			public LoadedIcon GetIcon(bool small)
			{
				if (small && _sm?.loadedIcon.icon)
				{
					return _sm.loadedIcon;
				}
				return loadedIcon;
			}
			
			[JsonIgnore] private LoadedIcon loadedIcon { get; set; }
			
			[OnDeserialized]
			private void OnDeserialized(StreamingContext ctx)
			{
				var bgColor = !string.IsNullOrEmpty(_bgColor) && ColorUtility.TryParseHtmlString(_bgColor, out var pColor)
				? new(pColor, pColor)
				: UnityConstants.BrowserColor;

				loadedIcon = new LoadedIcon()
				{
					tint = PVParse.ParseColor(_tint, Color.white),
					coords = PVParse.ParseRect(_uv, new Rect(0f, 0f, 1f, 1f)),
					icon = LoadTexture(_iconGUID),
					bgColor = bgColor
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
				return new IconFilter_Type(rule[2..]);
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
			public IconFilter_GUID(string guid)
			{
				this.guid = guid;
			}

			public override bool IsMatch(string inGUID, string path) => this.guid == inGUID;
			
			private readonly string guid;
		}
		
		
	}
}