// smidgens @ github

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using System;
	using System.Collections.Generic;
	using System.Runtime.Serialization;
	using Newtonsoft.Json;
	using UnityEngine;

	/// <summary>
	/// JSON file, menu config
	/// </summary>
	internal sealed class PVMenuProfile
	{
		internal PVMenuProfile(){}

		public MenuGenContext BuildMenu()
		{
			var ctx = new MenuGenContext(GetMenuItems(), _variables);
			if (_root == null)
			{
				return ctx;
			}

			if (!_init)
			{
				_root.OnInit(ctx);
			}
			_root.PopulateMenu(string.Empty, ctx);
			return ctx;
		}
		
		public static PVMenuProfile FromJSON(string data)
		{
			return JsonConvert.DeserializeObject<PVMenuProfile>(data, GetSerializationSettings());
		}

		private static JsonSerializerSettings GetSerializationSettings()
		{
			_cachedSerializationSettings ??= new JsonSerializerSettings
			{
				TypeNameHandling = TypeNameHandling.All,
				Converters = new List<JsonConverter>()
				{
					new SubclassJsonConverter<PVMenuProfileItem>(defaultType:typeof(MPItem_Group)),
				},
				MissingMemberHandling = MissingMemberHandling.Ignore,
				SerializationBinder = new TypeAliasSerializationBinder(typeof(PVMenuProfileItem))
			};
			return _cachedSerializationSettings;
		}

		private static JsonSerializerSettings _cachedSerializationSettings;

		[JsonProperty("scope")] private string _scopeRaw { get; set; }
		[JsonProperty("excludeMenus")] private string[] _excludeMenus { get; set; } = Array.Empty<string>();
		[JsonProperty("variables")] private Dictionary<string, object> _variables;
		[JsonProperty("root")] private MPItem_Group _root { get; set; }

		private bool _init;

		[OnDeserialized]
		private void OnDeserialized(StreamingContext context)
		{
		}

		private List<string> GetMenuItems()
		{
			var scope = _scopeRaw == "scene"
			? EOverrideScope.Scene
			: EOverrideScope.Project;
			var mPath = scope == EOverrideScope.Project ? "Assets" : "GameObject";
			List<string> items = new();
			foreach (var path in UnityUtility.GetSubmenus(mPath))
			{
				if (!_excludeMenus.MatchWildcard(path))
				{
					items.Add(path);
				}
			}
			return items;
		}

		
	}
}