// smidgens @ github

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using System;
	using System.Collections.Generic;
	using UnityEngine;

	public sealed class MenuGenNode
	{
		internal readonly string name;
		internal Texture2D icon;
		internal Action fn;
		internal Func<bool> enabledFn;
		internal MenuGenNode root { get; private set; }
		internal MenuGenNode parent { get; private set; }
		internal string path { get; private set; }
		internal int nestedCount { get; private set; }
		private readonly List<MenuGenNode> _children = new();

		public bool IsEnabled()
		{
			return fn != null && (enabledFn?.Invoke() ?? true);
		}

		public void Traverse(Action<string, MenuGenNode> onNode)
		{
			foreach (var c in _children)
			{
				if (c == null)
				{
					onNode.Invoke(path, null);
				}
				else
				{
					c.Traverse(onNode);
				}
			}
			// leaf node
			if (fn != null)
			{
				onNode.Invoke(path, this);
			}
		}

		public void AddNullChild()
		{
			_children.Add(null);
		}

		internal MenuGenNode(string name)
		{
			this.name = name;
			path = name;
		}

		private MenuGenNode FindImmediateChild(string cName)
		{
			foreach (var c in _children)
			{
				if (c?.name == cName)
				{
					return c;
				}
			}
			return null;
		}

		private void AddNestedCount()
		{
			var node = this;
			while (node != null)
			{
				node.nestedCount++;
				node = node.parent;
			}
		}

		internal MenuGenNode GetOrCreateChild(string cPath)
		{
			var segments = cPath.Split('/');
			var currentNode = this;

			var count = 0;
			foreach (var s in segments)
			{
				count++;
				var lastNode = currentNode;
				currentNode = lastNode.FindImmediateChild(s);
				if (currentNode == null)
				{
					var absPath = string.Join('/', segments, 0, count);
					if (path.Length > 0)
					{
						absPath = path + "/" + absPath;
					}
					currentNode = new MenuGenNode(s)
					{
						parent = lastNode,
						path = absPath,
						root = root ?? this // if root is null we are the root
					};
					currentNode.AddNestedCount(); // might be better to do this lazily, we're not using this always
					lastNode._children.Add(currentNode);
				}
			}

			return currentNode;
		}
	}


}

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using System;
	using System.Collections.Generic;
	using UnityEditor;
	using UnityEngine;

	public sealed class MenuGenContext
	{
		public MenuGenContext(IReadOnlyList<string> menuItems)
		{
			this.menuItems = menuItems;
		}

		public GenericMenu ToGenericMenu()
		{
			var m = new GenericMenu
			{
				allowDuplicateNames = true
			};

			root.Traverse((p, n) =>
			{
				if (n == null)
				{
					var sepPath = p.Length > 0 ? p + "/" : p;
					m.AddSeparator(sepPath);
					return;
				}
				if (n.fn == null)
				{
					return;
				}
				var oEnabled = n.IsEnabled();
				if (!oEnabled)
				{
					m.AddDisabledItem(new GUIContent(n.path));
					return;
				}
				m.AddItem(new GUIContent(n.path), false, () => n.fn.Invoke());
			});
			return m;
		}

		public void AddDivider(string path)
		{
			path = SanitizePath(path);
			(path.Length == 0 ? root : root.GetOrCreateChild(path))
			.AddNullChild();
		}

		public void AddItem(string path, Action fn, Func<bool> enabledFn = null, string icon = null)
		{
			var item = root.GetOrCreateChild(SanitizePath(path));
			item.fn = fn;
			item.enabledFn = enabledFn;
			if (!string.IsNullOrEmpty(icon))
			{
				item.icon = icons.GetIconOrCached(icon);
			}
		}

		public IReadOnlyList<string> menuItems { get; }
		internal readonly MenuGenNode root = new(string.Empty);
		internal readonly IconStore icons = new ();

		private static string SanitizePath(string path)
		{
			if (path.Length == 0)
			{
				return path;
			}
			int sIndex = 0;
			while (sIndex < path.Length && path[sIndex] == '/')
			{
				sIndex++;
			}
			if (sIndex >= path.Length)
			{
				return string.Empty;
			}
			if (sIndex > 0)
			{
				return path.Substring(sIndex);
			}
			// maybe here: remove invalid chars and stuff like htat
			return path;
		}

	}
}

