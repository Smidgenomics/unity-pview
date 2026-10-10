// smidgens @ github

// ReSharper disable TailRecursiveCall

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using System;
	using System.Collections.Generic;
	using System.Text.RegularExpressions;
	using UnityEditor;
	using UnityEngine;

	[System.Serializable]
	internal sealed class IconGenContext : IStaleInfo
	{
		public IconGenContext(in IconDefaults defaults, Dictionary<string, IconSettings> rules)
		{
			foreach (var (rule, settings) in rules)
			{
				AddRule(rule, settings);
			}

			if (defaults.folderIcon != null)
			{
				_defaultFolder = (true, defaults.folderIcon.LoadIcon());
			}
		}

		public void SetSourcePaths(IEnumerable<string> paths)
		{
			_sourceFiles = new FileEditCheck(paths);
		}

		public bool TryGetIcon(string guid, bool small, out LoadedIcon ico)
		{
			if (_guidIcons.TryGetValue(guid, out var val))
			{
				ico = small ? val.Item2 : val.Item1;
				return true;
			}

			var path = AssetDatabase.GUIDToAssetPath(guid);
			var fArgs = new FilterArgs
			{
				path = path,
				isFolder = AssetDatabase.IsValidFolder(path),
				type = _hasTypeFilters ? AssetDatabase.GetMainAssetTypeAtPath(path) : typeof(UnityEngine.Object)
			};
			var i = Filter(fArgs);
			if (i > -1)
			{
				ico = small ? _icons[i].Item2 : _icons[i].Item1;
				return true;
			}

			if (fArgs.isFolder && _defaultFolder.Item1)
			{
				ico = _defaultFolder.Item2;
				return true;
			}

			ico = default;
			return false;
		}

		public void AddRule(string rule, IconSettings settings)
		{
			if (rule.Contains(';'))
			{
				foreach (var r in rule.Split(';'))
				{
					AddRule(r.Trim(), settings);
				}
				return;
			}

			var mainIcon = settings.LoadIcon();
			var smallIcon = settings.smallVariant?.LoadIcon() ?? mainIcon;
			if (rule.IsGUID32())
			{
				_guidIcons[rule] = (mainIcon, smallIcon);
				return;
			}

			FilterFn fn;

			var folderOnly = rule.StartsWith("f:");
			rule = folderOnly ? rule.Replace("f:", "") : rule;

			if (rule.StartsWith("t:"))
			{
				_hasTypeFilters = true;
				fn = GetTypeFilterFn(rule[2..]);
			}
			else
			{
				// if path is absolute, we might be able to cache it by guid
				if (!rule.Contains('*') && !rule.Contains('^'))
				{
					var possibleGUID = AssetDatabase.AssetPathToGUID(rule);
					if (!string.IsNullOrEmpty(possibleGUID))
					{
						AddRule(possibleGUID, settings);
						return;
					}
				}
				fn = GetPathFilterFn(rule);
			}
			
			_filters.Add(new FilterInfo
			{
				fn = fn,
				folderOnly = folderOnly
			});
			_icons.Add((mainIcon, smallIcon));
		}

		internal ref struct FilterArgs
		{
			public string path;
			public bool isFolder;
			public Type type;
		}

		private delegate bool FilterFn(in FilterArgs args);

		private int Filter(in FilterArgs args)
		{
			var i = -1;
			foreach (var fInfo in _filters)
			{
				i++;
				if (!args.isFolder && fInfo.folderOnly)
				{
					continue;
				}
				if (fInfo.fn.Invoke(args))
				{
					return i;
				}
			}
			return -1;
		}

		private struct FilterInfo
		{
			public FilterFn fn;
			public bool folderOnly;
		}

		private bool _hasTypeFilters;

		private (bool, LoadedIcon) _defaultFolder;
		private FileEditCheck _sourceFiles;
		private Dictionary<string, (LoadedIcon, LoadedIcon)> _guidIcons = new();
		private List<FilterInfo> _filters = new();
		private List<(LoadedIcon, LoadedIcon)> _icons = new(); // normal/small

		private static FilterFn GetPathFilterFn(string rule)
		{
			var regex = RegexFromString(rule);
			if (regex != null)
			{
				return (in FilterArgs args) => regex.IsMatch(args.path);
			}
			return (in FilterArgs args) => rule == args.path;
		}

		private static FilterFn GetTypeFilterFn(string rule)
		{
			var exactType = rule.Contains(',') ? Type.GetType(rule) : null;
			if (exactType != null)
			{
				if (exactType.IsAbstract)
				{
					return (in FilterArgs args) => exactType.IsAssignableFrom(args.type);
				}
				return (in FilterArgs args) => args.type == exactType;
			}

			var regex = RegexFromString(rule);

			// match using fullname
			if (regex != null)
			{
				return (in FilterArgs args) => regex.IsMatch(args.type.FullName!);
			}
			// match by name only
			return (in FilterArgs args) => args.type.Name == rule;
		}

		private static Regex RegexFromString(string str)
		{
			if (str.Contains('*'))
			{
				return new Wildcard(str);
			}
			if (str.StartsWith('^'))
			{
				return new Regex(str);
			}
			return null;
		}

		public bool IsStale()
		{
			return _sourceFiles.AnyEdited();
		}
	}
}