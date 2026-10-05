// smidgens @ github

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using System;
	using System.Text.RegularExpressions;
	using Newtonsoft.Json;

	// executes editor menu item(s)
	[TypeAlias("menu")]
	[TypeAlias("mi")]
	internal sealed class MPItem_MenuItem : PVMenuProfileItem
	{
		public override void PopulateMenu(string currentPath, MenuGenContext context)
		{
			if (currentPath.Length > 0)
			{
				currentPath += "/";
			}

			if (_regex != null)
			{
				if (!string.IsNullOrEmpty(label))
				{
					currentPath += label + "/";
				}
				foreach (var item in context.menuItems)
				{
					if (_regex.IsMatch(item))
					{
						var lb = item[(item.LastIndexOf('/') + 1)..];
						AddMenuItem(currentPath + lb, item, context);
					}
				}
			}
			else // single item
			{
				if (IsPathValid(_path, context))
				{
					// default to menu name if label is empty
					var mLabel = string.IsNullOrEmpty(label)
					? _path[(_path.LastIndexOf('/') + 1)..]
					: label;
					AddMenuItem(currentPath + mLabel, _path, context);
				}
			}
		}

		public override void OnInit(MenuGenContext context)
		{
			base.OnInit(context);
			_path ??= string.Empty;
			_regex = TryGetRegex(_path);
			_hideDisabled |= context.GetVariableOrDefault("hideDisabledMenus", false);
		}

		protected override void OnDeserialized()
		{
			
		}

		// either single item or wildcard pattern
		[field:InjectVariables]
		[JsonProperty("path")] private string _path { get; set; }
		[JsonProperty("hideDisabled")] private bool _hideDisabled;

		private Regex _regex;

		private static Regex TryGetRegex(string str)
		{
			if (str.Length > 2 && str.StartsWith("r:"))
			{
				return new Regex(str.Substring(2));
			}
			if (str.Contains('*'))
			{
				return new Wildcard(str);
			}
			return null;
		}

		private static bool IsPathValid(string path, MenuGenContext ctx)
		{
			foreach (var mi in ctx.menuItems)
			{
				if (mi == path)
				{
					return true;
				}
			}
			return false;
		}

		private static bool GetTrue() => true;

		private void AddMenuItem(string mPath, string unityMenuPath, MenuGenContext ctx)
		{
			var enableFn = CreateMenuPredicate(unityMenuPath);
			Func<bool> visFn = _hideDisabled
			? enableFn.Invoke
			: GetTrue;
			ctx.AddItem(mPath, CreateMenuAction(unityMenuPath), enableFn, visFn);
		}

		private static Action CreateMenuAction(string menuItem) => () => UnityUtility.ExecuteMenu(menuItem);

		private static Func<bool> CreateMenuPredicate(string menuItem) => () => UnityUtility.CanExecuteMenu(menuItem);
	}
}
