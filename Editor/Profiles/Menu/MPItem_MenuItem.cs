// smidgens @ github

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using System;
	using System.Text.RegularExpressions;
	using Newtonsoft.Json;

	// executes editor menu item(s)
	[TypeAlias("menu")]
	internal sealed class MPItem_MenuItem : PVMenuProfileItem
	{
		// either single item or wildcard pattern
		[JsonProperty] public string path { get; internal set; }

		private Regex _regex;

		protected override void OnDeserialized()
		{
			path ??= string.Empty;
			_regex = TryGetRegex(path);
			if (_regex == null && IsPattern(path))
			{
				_regex = new Wildcard(path);
			}
		}

		public override void PopulateMenu(string currentPath, MenuGenContext context)
		{
			if (string.IsNullOrEmpty(path))
			{
				return;
			}

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
			else // single itemj
			{
				// default to menu name if label is empty
				var mLabel = string.IsNullOrEmpty(label)
				? path[(path.LastIndexOf('/') + 1)..]
				: label;
				AddMenuItem(currentPath + mLabel, path, context);
			}
		}

		private static Regex TryGetRegex(string str)
		{
			if (str.Length > 2 && str.StartsWith("r:"))
			{
				return new Regex(str.Substring(2));
			}
			return null;
		}

		private static bool IsPattern(string str)
		{
			return str.Contains('*');
		}

		private static void AddMenuItem(string mPath, string unityMenuPath, MenuGenContext ctx)
		{
			ctx.AddItem(mPath, CreateMenuAction(unityMenuPath), CreateMenuPredicate(unityMenuPath));
		}

		private static Action CreateMenuAction(string menuItem)
		{
			return () => UnityUtility.ExecuteMenu(menuItem);
		}

		private static Func<bool> CreateMenuPredicate(string menuItem)
		{
			return () => UnityUtility.CanExecuteMenu(menuItem);
		}
	}
}
