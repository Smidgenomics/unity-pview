// smidgens @ github

// ReSharper disable SuggestVarOrType_SimpleTypes

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using System;
	using UnityEngine;
	using System.IO;
	using UnityEditor;

	internal static class ProjectView
	{
		public static void OpenUserPrefs()
		{
			SettingsService.OpenUserPreferences("Preferences/" + PVConstants.SETTINGS_TAB_PATH);
		}

		public static void OpenProjectPrefs()
		{
			SettingsService.OpenProjectSettings("Project/" + PVConstants.SETTINGS_TAB_PATH);
		}
		
		public static void OnBrowserGUI(string guid, Rect pos)
		{
			DrawIcons(pos, guid);
			HandleMenu();
		}

		private static void DrawIcons(Rect pos, string guid)
		{
			var icoFile = GetActiveIconFile();
			if (icoFile == null)
			{
				return;
			}
			if (icoFile.TryGetIconByGUID(guid, out var icon))
			{
				IconGUI.DrawIcon(pos, icon.icon, icon.coords, tint:icon.tint);
			}
		}

		private static void HandleMenu()
		{
			// not a proper context event
			if (!IsUsableContextEvent(Event.current))
			{
				return;
			}
			var ctx = GetActiveMenu(Event.current);
			var m = ctx?.ToGenericMenu();

			// menu is null -> show default unity
			if (m != null)
			{
				Event.current.Use();
				m.ShowAsContext();
			}
		}

		private static bool IsUsableContextEvent(Event e)
		{
			if (e == null)
			{
				return false;
			}

			// straight up context event
			if (e.type == EventType.ContextClick)
			{
				return true;
			}

			// awkward workaround for empty folders
			// not triggering a proper ctx event
			// note: may make this disableable in settings
			if (e.button == 1 && e.type == EventType.Ignore)
			{
				var r = UnityUtility.GetProjectBrowserListArea();
				return r.Contains(e.mousePosition);
			}

			return false;
		}

		private static PVIconProfile GetActiveIconFile()
		{
			return PVSettings_User.instance._defaultIcons switch
			{
				EOverrideBehaviour.UnityDefault => null,
				EOverrideBehaviour.ProjectProfile => GetIconsFromJSONPath(PVSettings_Project.instance._iconProfile, ref _cachedProjectIcons),
				EOverrideBehaviour.UserProfile => GetIconsFromJSONPath(PVSettings_User.instance._iconProfile, ref _cachedUserIcons),
				_ => null
			};
		}

		private static CachedLoad<MenuGenContext> _cachedUserMenu;
		private static CachedLoad<MenuGenContext> _cachedProjectMenu;
		private static CachedLoad<PVIconProfile> _cachedUserIcons;
		private static CachedLoad<PVIconProfile> _cachedProjectIcons;

		private struct CachedLoad<T>
		{
			public readonly string file;
			public readonly long timestamp;
			public readonly T data;

			public CachedLoad(string f, long ts, T d)
			{
				file = f;
				timestamp = ts;
				data = d;
			}
		}

		private static MenuGenContext GetProjectMenu()
		{
			return GetMenuFromJSON(PVSettings_Project.instance._menuProfile, ref _cachedProjectMenu);
		}
		
		private static MenuGenContext GetUserMenu()
		{
			return GetMenuFromJSON(PVSettings_User.instance._menuProfile, ref _cachedUserMenu);
		}

		private static MenuGenContext PickMenu(EOverrideBehaviour b)
		{
			return b switch
			{
				EOverrideBehaviour.UnityDefault => null,
				EOverrideBehaviour.ProjectProfile => GetProjectMenu(),
				EOverrideBehaviour.UserProfile => GetUserMenu(),
				_ => null
			};
		}

		// select menu handler from context
		private static MenuGenContext GetActiveMenu(Event e)
		{
			var uSettings = PVSettings_User.instance;
			EOverrideBehaviour behaviour = uSettings._defaultMenu;
			ref readonly var mods = ref uSettings._modifierBehaviours;

			if (mods.AreAnyEnabled())
			{
				if (e.control && mods.ctrl.enabled)
				{
					behaviour = mods.ctrl.value;
				}
				else if (e.shift && mods.shift.enabled)
				{
					behaviour = mods.shift.value;
				}
				else if (e.alt && mods.alt.enabled)
				{
					behaviour = mods.alt.value;
				}
			}
			return PickMenu(behaviour);
		}

		private static PVIconProfile IconsFromJSON(string json)
		{
			return PVIconProfile.FromJSON(json);
		}
		
		private static MenuGenContext MenuFromJSON(string json)
		{
			return PVMenuProfile.FromJSON(json).BuildMenu();
		}

		private static PVIconProfile GetIconsFromJSONPath(string file, ref CachedLoad<PVIconProfile> cache)
		{
			return LoadCachedFromJSON(file, IconsFromJSON, ref cache);
		}
		
		private static MenuGenContext GetMenuFromJSON(string file, ref CachedLoad<MenuGenContext> cache)
		{
			return LoadCachedFromJSON(file, MenuFromJSON, ref cache);
		}

		private static T LoadCachedFromJSON<T>(string file, Func<string,T> factory, ref CachedLoad<T> cache)
		{
			var absFilePath = $"{PVConstants.PROJECT_ROOT}/{file}";
			var exists = File.Exists(absFilePath);
			var lastEdit = exists
			? File.GetLastWriteTimeUtc(absFilePath).ToFileTime()
			: -1;

			if (lastEdit == cache.timestamp && file != cache.file)
			{
				cache = default;
			}

			if (lastEdit != cache.timestamp)
			{
				if (exists)
				{
					try
					{
						cache = new CachedLoad<T>(file, lastEdit, factory.Invoke(File.ReadAllText(absFilePath)));
						return cache.data;
					}
					catch (Exception e)
					{
						Debug.LogError(e);
						Debug.LogError($"Error parsing profile at '{absFilePath}': Using defaults");
						cache = new CachedLoad<T>(file, lastEdit, default);
					}
				}
				cache = new CachedLoad<T>(file, lastEdit, default);
			}
			return cache.data;
		}

		
		
	}
}