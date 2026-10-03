// smidgens @ github

#pragma warning disable 0414

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using System;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEditor.IMGUI.Controls;

	[System.Serializable]
	internal sealed class GenericContextMenu : AdvancedDropdown
	{
		public GenericContextMenu() : this(new AdvancedDropdownState())
		{
			
		}

		public GenericContextMenu(AdvancedDropdownState state) : base(state)
		{
			
		}

		public void AddItem(string path, Action fn, Texture2D icon = null)
		{
			var nPath = path.Split('/');
			var n = GetOrCreateNode(nPath);
			n.fn = fn;
			n.icon = icon;
		}

		private sealed class ItemNode
		{
			public string name;
			public bool enabled;
			public Texture2D icon;
			public Action fn;
			public IReadOnlyList<ItemNode> children => _children;
			private readonly List<ItemNode> _children = new();

			public ItemNode GetOrCreateChild(string cName)
			{
				var n = _children.Find(x => x.name == cName);
				if (n == null)
				{
					n = new ItemNode
					{
						name = cName
					};
				}
				return n;
			}
		}

		private readonly ItemNode _root = new()
		{
			name = "Items"
		};

		protected override void ItemSelected(AdvancedDropdownItem item)
		{
			var ctx = (item as GenericContextMenuItem)!;
			ctx.data.fn.Invoke();
		}

		private sealed class GenericContextMenuItem : AdvancedDropdownItem
		{
			public GenericContextMenuItem(ItemNode node) : base(node.name)
			{
				data = node;
				icon = node.icon;
				enabled = node.enabled;
			}
			public ItemNode data { get; }
		}

		protected override AdvancedDropdownItem BuildRoot()
		{
			return GetItem(_root);
		}

		private AdvancedDropdownItem GetItem(ItemNode node)
		{
			var item = new GenericContextMenuItem(node);
			foreach (var c in node.children)
			{
				item.AddChild(GetItem(c));
			}
			return item;
		}

		private ItemNode GetOrCreateNode(string[] path)
		{
			var currentNode = _root;
			foreach (var t in path)
			{
				currentNode = currentNode.GetOrCreateChild(t);
			}
			return currentNode;
		}
	}
}