// smidgens @ github

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using System;
	using System.Collections.Generic;
	using Newtonsoft.Json;

	/// <summary>
	/// Icon JSON file
	/// </summary>
	[Serializable]
	internal sealed class PVIconProfile
	{
		public static PVIconProfile LoadFromPath(string path)
		{
			var ctx = ProfileLoadContext.New(path);
			var nextPath = ctx.DequeuePath();
			while (nextPath != null)
			{
				var data = PVUtils.ReadRelativeFile(nextPath);
				if (data == null)
				{
					nextPath = ctx.DequeuePath();
					continue;
				}
				var prof = JsonConvert.DeserializeObject<PVIconProfile>(data, GetSerializationSettings());
				ctx.AddProfile(nextPath, prof);
				nextPath = ctx.DequeuePath();
			}

			if (ctx.loadedProfiles.Count == 0)
			{
				return null;
			}

			var baseProf = ctx.loadedProfiles[^1];

			for (var i = ctx.loadedProfiles.Count - 2; i >= 0; i--)
			{
				baseProf.MergeOther(ctx.loadedProfiles[i]);
			}
			baseProf._sourcePaths = ctx.loadedPaths;
			return baseProf;
		}

		public IconGenContext CreateContext()
		{
			var ctx = new IconGenContext(_defaults, _rules);
			ctx.SetSourcePaths(_sourcePaths ?? Array.Empty<string>());
			foreach (var (r,v) in _rules)
			{
				if (v == null)
				{
					continue;
				}
				ctx.AddRule(r, v);
			}
			return ctx;
		}

		private void MergeOther(PVIconProfile otherProf)
		{
			if (otherProf._defaults.folderIcon != null)
			{
				_defaults.folderIcon = otherProf._defaults.folderIcon;
			}

			// add/overwrite rules
			foreach (var (rule, ico) in otherProf._rules)
			{
				_rules[rule] = ico;
			}
		}

		private struct ProfileLoadContext
		{
			public IReadOnlyList<PVIconProfile> loadedProfiles => _loadedProfiles;
			public IEnumerable<string> loadedPaths => _loadedPaths;

			public readonly string DequeuePath()
			{
				return _pathQueue.Count > 0 ? _pathQueue.Dequeue() : null;
			}

			public static ProfileLoadContext New(string mainPath)
			{
				var ctx = new ProfileLoadContext
				{
					_pathQueue = new(){},
					_loadedProfiles = new(),
					_loadedPaths = new()
				};
				ctx._pathQueue.Enqueue(mainPath);
				return ctx;
			}

			public readonly void AddProfile(string path, PVIconProfile profile)
			{
				_loadedPaths.Add(path);
				_loadedProfiles.Add(profile);
				if (profile._include != null)
				{
					foreach (var p in profile._include)
					{
						if (!_loadedPaths.Contains(p))
						{
							_pathQueue.Enqueue(p);
						}
					}
				}
			}

			// path -> profile
			private Queue<string> _pathQueue;
			private HashSet<string> _loadedPaths;
			private List<PVIconProfile> _loadedProfiles;
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
		[JsonProperty("include")] private string[] _include;
		[JsonProperty("defaults")] private IconDefaults _defaults;
		[JsonProperty("rules")] private Dictionary<string, IconSettings> _rules { get; set; } = new();

		private IEnumerable<string> _sourcePaths;
	}
}