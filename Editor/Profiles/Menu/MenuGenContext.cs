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
		internal Func<bool> visibilityFn;
		internal MenuGenNode root { get; private set; }
		internal MenuGenNode parent { get; private set; }
		internal string path { get; private set; }
		internal int nestedCount { get; private set; }
		private readonly List<MenuGenNode> _children = new();

		public bool IsVisible()
		{
			return visibilityFn?.Invoke() ?? true;
		}

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
			if (fn != null && IsVisible())
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
	using System.Reflection;
	using UnityEditor;
	using UnityEngine;

	public sealed class MenuGenContext
	{
		internal MenuGenContext(IReadOnlyList<string> menuItems, Dictionary<string,object> variables)
		{
			this.menuItems = menuItems;
			_variables = variables ?? new Dictionary<string, object>();
		}

		internal GenericMenu ToGenericMenu()
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

			if (m.GetItemCount() == 0)
			{
				m.AddDisabledItem(new GUIContent("No options"), false);
			}
			
			return m;
		}

		// injects variables to string fields marked with [InjectVariables]
		public void InjectVariables<T>(T ob) where T : class
		{
			var oType = ob.GetType();
			if (!_INJECT_FIELDS.TryGetValue(oType, out var fList))
			{
				var fields = oType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				var l = new List<FieldInfo>();
				fList = l;
				_INJECT_FIELDS.Add(oType, l);
				foreach (var f in fields)
				{
					if (!f.IsDefined(typeof(InjectVariablesAttribute)) || f.FieldType != typeof(string))
					{
						continue;
					}
					l.Add(f);
				}
			}
			foreach (var f in fList)
			{
				InjectVariables(f, ob);
			}
		}

		private void InjectVariables(FieldInfo field, object target)
		{
			var currVal = (string)field.GetValue(target);
			if (currVal == null || !currVal.Contains('$'))
			{
				return;
			}
			var newVal = InjectVariables(currVal);
			if (newVal != currVal)
			{
				field.SetValue(target, newVal);
			}
		}

		private readonly Dictionary<Type, IReadOnlyList<FieldInfo>> _INJECT_FIELDS = new();

		// formats string by inserting context variables
		public string InjectVariables(string str)
		{
			if (str == null || !str.Contains('$'))
			{
				return str;
			}
			// brute force, works for now
			foreach (var (k, v) in _variables)
			{
				if (!str.Contains('$'))
				{
					break;
				}
				str = str.Replace($"${{{k}}}", v.ToString());
			}
			return str;
		}

		// syntactic sugar, returns default value if variable is not found
		public T GetVariableOrDefault<T>(string name, T dValue)
		{
			return TryGetVariable<T>(name, out var v) ? v : dValue;
		}

		// looks up variable 
		public bool TryGetVariable<T>(string name, out T value)
		{
			if (_variables.TryGetValue(name, out var obVal) && obVal is T val)
			{
				value = val;
				return true;
			}
			value = default;
			return false;
		}

		public void AddDivider(string path)
		{
			path = SanitizePath(path);
			(path.Length == 0 ? root : root.GetOrCreateChild(path))
			.AddNullChild();
		}

		public void AddItem
		(
			string path,
			Action fn,
			Func<bool> enabledFn = null,
			Func<bool> visibilityFn = null,
			string icon = null
		)
		{
			var item = root.GetOrCreateChild(SanitizePath(path));
			item.fn = fn;
			item.enabledFn = enabledFn;
			item.visibilityFn = visibilityFn;
			if (!string.IsNullOrEmpty(icon))
			{
				item.icon = icons.GetIconOrCached(icon);
			}
		}

		public IReadOnlyList<string> menuItems { get; }
		internal readonly MenuGenNode root = new(string.Empty);
		internal readonly IconStore icons = new ();

		private readonly IReadOnlyDictionary<string, object> _variables;

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

