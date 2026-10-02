// smidgens @ github

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using System;
	using System.Collections.Generic;
	using System.Runtime.Serialization;
	using Newtonsoft.Json;

	/// <summary>
	/// JSON file, menu config
	/// </summary>
	internal sealed class PVMenuProfile
	{
		internal PVMenuProfile(){}

		public MenuGenContext BuildMenu()
		{
			var ctx = new MenuGenContext(GetMenuItems());
			if (root == null)
			{
				return ctx;
			}
			root.PopulateMenu(string.Empty, ctx);
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

		[JsonProperty("scope")] private string scopeRaw { get; set; }
		[JsonProperty] internal string[] excludeMenus { get; set; } = Array.Empty<string>();
		[JsonProperty] internal PVMenuProfileItem root { get; set; }
		[JsonIgnore] public EOverrideScope scope { get; private set; }
		
		[OnDeserialized]
		private void OnDeserialized(StreamingContext context)
		{
			if (!Enum.TryParse(typeof(EOverrideScope), scopeRaw, out var val))
			{
				val = EOverrideScope.Project;
			}
			scope = (EOverrideScope)val;
		}

		private List<string> GetMenuItems()
		{
			var mPath = scope == EOverrideScope.Project ? "Assets" : "GameObject";
			List<string> items = new();
			foreach (var path in UnityUtility.GetSubmenus(mPath))
			{
				if (!excludeMenus.MatchWildcard(path))
				{
					items.Add(path);
				}
			}
			return items;
		}

		
	}
}