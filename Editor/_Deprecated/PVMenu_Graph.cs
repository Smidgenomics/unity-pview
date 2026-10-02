// smidgens @ github

#pragma warning disable CS0618 // Type or member is obsolete
namespace Smidgenomics.Unity.ProjectView.Editor
{
	using System;
	using System.Collections.Generic;
	using UnityEditor;
	using UnityEditor.Callbacks;
	using UnityEngine;

	/// <summary>
	/// Menu graph asset
	/// </summary>
	// [CreateAssetMenu(menuName = PVConstants.CREATE_ROOT + "Menu/Graph")]
	[Obsolete("Replaced with JSON profiles")]
	internal partial class PVMenu_Graph : PVMenu
	{
		// entry node
		public MenuNode RootNode => _rootNode;

		// total number of nodes, used or not
		public int NodeCount => _nodes.Length;

		// total number of edges
		public int EdgeCount => _edges.Length;

		// edge at index
		public NEdge GetEdgeAt(int i) => _edges.IsOutOfBounds(i) ? null : _edges[i];

		// node at index
		public MenuNode GetNodeAt(int i) => _nodes.IsOutOfBounds(i) ? null : _nodes[i];

		public override GenericMenu GetMenu()
		{
			var m = new GenericMenu
			{
				allowDuplicateNames = true
			};

			if (!string.IsNullOrEmpty(_menuTitle))
			{
				m.AddDisabledItem(new GUIContent(_menuTitle));
				m.AddSeparator(string.Empty);
			}

			var items = new List<Tuple<GUIContent, Action>>();

			if (!RootNode)
			{
				m.AddDisabledItem(new GUIContent("Empty"));
				return m;
			}

			var visited = new Dictionary<MenuNode, bool>();

			void Traverse(MenuNode node, string path)
			{
				// circular check...
				if (!visited.TryAdd(node, true))
				{
					return;
				}
				var handler = node.GetInvokeHandler();

				if (node.Outputs == 0)
				{
					items.Add(new Tuple<GUIContent, Action>(new GUIContent(path), handler));
				}

				// outgoing nodes
				foreach (var exitNode in GetExits(node))
				{
					Traverse(exitNode, path + "/" + GetNodeText(exitNode));
				}
			}

			visited[RootNode] = true;

			foreach (var e in GetExits(RootNode))
			{
				Traverse(e, GetNodeText(e));
			}

			foreach (var l in items)
			{
				if (l.Item2 != null)
				{
					m.AddItem(l.Item1, false, () => l.Item2.Invoke());
				}
				else { m.AddDisabledItem(l.Item1); }
			}
			return m;
		}

		public bool AreNodesConnected(MenuNode n1, MenuNode n2)
		{
			return Array.FindIndex(_edges, x => x.HasNodes(n1, n2)) > -1;
		}

		public MenuNode GetLeftMostNode()
		{
			var x = float.MaxValue;
			MenuNode node = null;
			foreach (var n in _nodes)
			{
				if (n.Position.x < x) { node = n; }
			}
			return node;
		}

		public int IndexOfNode(MenuNode n)
		{
			return Array.FindIndex(_nodes, x => x == n);
		}

		public bool HasNodes(params MenuNode[] nodes)
		{
			foreach (var n in nodes)
			{
				if (!n || IndexOfNode(n) < 0) { return false; }
			}
			return true;
		}

		[SerializeField] internal string _menuTitle = "";
		[SerializeField, HideInInspector] private MenuNode _rootNode;
		[SerializeField, HideInInspector] private NEdge[] _edges = Array.Empty<NEdge>();
		[SerializeField, HideInInspector] private MenuNode[] _nodes = Array.Empty<MenuNode>();
		[SerializeField, HideInInspector] private int _lastModified = -1;
		
#if UNITY_6000_0_OR_NEWER
		[OnOpenAsset]
		private static bool OnEditMenuGraph(EntityId entityId, int line)
		{
			return OpenGraphAsset(EditorUtility.EntityIdToObject(entityId));
		}
#else
		[OnOpenAsset]
		private static bool OnEditMenuGraph(int instanceID, int line)
		{
			return OpenGraphAsset(EditorUtility.InstanceIDToObject(instanceID));
		}
#endif

		private static bool OpenGraphAsset(UnityEngine.Object ob)
		{
			if (ob?.GetType() != typeof(PVMenu_Graph))
			{
				return false;
			}
			EditMenuGraph.Open((PVMenu_Graph)ob);
			return true;
		}

		// finds possible exit nodes
		private List<MenuNode> GetExits(MenuNode node)
		{
			var exits = new List<MenuNode>();
			if (node.Outputs == 0)
			{
				return exits;
			}
			for (var i = 0; i < _edges.Length; i++)
			{
				var e = _edges[i];
				if (e.N1 != node || !e.N2)
				{
					continue;
				}
				exits.Add(e.N2);
			}
			exits.Sort((a, b) =>
			{
				var w1 = a.SortWeight;
				var w2 = b.SortWeight;
				if (Mathf.Approximately(w1, w2))
				{
					return 0;
				}
				return w1 > w2 ? 1 : -1;
			});

			return exits;
		}

		private static string GetNodeText(MenuNode node)
		{
			if (node.GetType() == typeof(MenuSeparator)) { return ""; }
			return node.Label;
		}


	}
}

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using System;
	using UnityEngine;

	partial class PVMenu_Graph
	{
		[Serializable]
		internal sealed class NEdge
		{
			public MenuNode N1 => _n1;
			public MenuNode N2 => _n2;
			public int P1 => _p1;
			public int P2 => _p2;

			public bool IsValid => !(!_n1 || !_n2 || _p1 == 0 || _p2 == 0);

			public NEdge(MenuNode n1, MenuNode n2, int p1, int p2)
			{
				_n1 = n1;
				_n2 = n2;
				_p1 = p1;
				_p2 = p2;
			}

			/// <summary>
			/// Check if edge connects given nodes (in any order)
			/// </summary>
			public bool HasNodes(MenuNode n1, MenuNode n2)
			{
				if (!n1 || !n2) { return false; }
				return (n1 == _n1 && n2 == _n2) || (n1 == _n2 && n2 == _n1);
			}

			public bool HasNode(MenuNode n)
			{
				if (!n) { return false; }
				return _n1 == n || _n2 == n;
			}

			[SerializeField] private MenuNode _n1;
			[SerializeField] private MenuNode _n2;
			[SerializeField] private int _p1;
			[SerializeField] private int _p2;
		}
	}
}


namespace Smidgenomics.Unity.ProjectView.Editor
{
	using System;
	using System.Linq;
	using UnityEditor;
	using UnityEngine;

	partial class PVMenu_Graph
	{
		/// <summary>
		/// Editing API with Undo tracking
		/// </summary>
		public static class WithUndo
		{
			public const string NODES_FIELD = nameof(_nodes);
			public const string MODIFIED_FIELD = nameof(_lastModified);

			/// <summary>
			/// Sets root/start node
			/// </summary>
			public static bool SetRootNode(PVMenu_Graph g, MenuNode n)
			{
				Undo.RecordObject(g, nameof(SetRootNode));
				g._rootNode = n;
				MarkChanges(g);
				return true;
			}

			public static bool AddNode(PVMenu_Graph g, Type nodeType, Vector2 pos)
			{
				if (!typeof(MenuNode).IsAssignableFrom(nodeType))
				{
					return false;
				}

				var node = (MenuNode)g.InstantiateNestedSO(nodeType, hide: true, name: string.Empty);
				if (!node)
				{
					return false;
				}

				// apply position
				MenuNode.WithUndo.SetPosition(node, pos);
				// apply changes
				Undo.RecordObject(g, "Add node");
				var nodes = g._nodes.ToList();
				nodes.Add(node);
				g._nodes = nodes.ToArray();
				MarkChanges(g);
				return true;
			}

			/// <summary>
			/// Deletes a node and all edges using it
			/// </summary>
			public static bool DeleteNode(PVMenu_Graph g, MenuNode n)
			{
				// node isn't in graph
				if (!g.HasNodes(n)) { return false; }
				// remove nodes
				var nodes = g._nodes.ToList();
				nodes.Remove(n);
				// remove edges
				var edges = g._edges.ToList();
				var removedEdges = edges.RemoveAll(x => x.HasNode(n));
				// apply changes
				Undo.RecordObject(g, nameof(DeleteNode));
				g._nodes = nodes.ToArray();
				if (removedEdges > 0) { g._edges = edges.ToArray(); }
				Undo.DestroyObjectImmediate(n);
				MarkChanges(g);
				return true;
			}

			/// <summary>
			/// Connects two nodes
			/// </summary>
			public static bool AddEdge(PVMenu_Graph g, MenuNode n1, MenuNode n2)
			{
				// connect to self?
				if (n1 == n2)
				{
					return false;
				}
				// nodes not found in menu
				if (!g.HasNodes(n1, n2))
				{
					return false;
				}
				// are nodes already connected
				if (g.AreNodesConnected(n1, n2))
				{
					return false;
				}

				// apply changes
				Undo.RecordObject(g, nameof(AddEdge));
				var edges = g._edges.ToList();
				var e = new NEdge(n1, n2, -1, 1);
				edges.Add(e);
				g._edges = edges.ToArray();
				MarkChanges(g);
				//AssetDatabase.SaveAssetIfDirty(g);
				return true;
			}

			/// <summary>
			/// Deletes all edges in graph
			/// </summary>
			public static bool DeleteAllEdges(PVMenu_Graph g)
			{
				// nothing to delete
				if (g._edges.Length == 0) { return false; }
				// clear edges and apply
				Undo.RecordObject(g, nameof(DeleteAllEdges));
				g._edges = Array.Empty<NEdge>();
				MarkChanges(g);
				return true;
			}

			/// <summary>
			/// Deletes edge at specific index
			/// </summary>
			public static bool DeleteEdge(PVMenu_Graph g, int i)
			{
				if (g._edges.IsOutOfBounds(i)) { return false; }

				// apply changes
				Undo.RecordObject(g, nameof(DeleteEdge));

				var edges = g._edges.ToList();
				edges.RemoveAt(i);
				g._edges = edges.ToArray();

				MarkChanges(g);

				return true;
			}

			/// <summary>
			/// Delete all edges between nodes
			/// </summary>
			public static bool DeleteEdges(PVMenu_Graph g, MenuNode n1, MenuNode n2)
			{
				// remove all matching edges
				var edges = g._edges
				.Where(x => !(!x.IsValid || (x.N1 == n1 && x.N2 == n2))) // filter
				.ToArray();

				if (edges.Length == g._edges.Length)
				{
					return false;
				}
				Undo.RecordObject(g, nameof(DeleteEdge));
				g._edges = edges;
				MarkChanges(g);
				return true;
			}

			public static int DeleteInvalidEdges(PVMenu_Graph g)
			{
				var edges = g._edges
				.Where(x => x.IsValid) // retain valid edges
				.ToArray();
				var count = g._edges.Length - edges.Length;
				// if no invalids were found
				if (count == 0) { return 0; }
				// overwrite edges
				Undo.RecordObject(g, nameof(DeleteInvalidEdges));
				g._edges = edges;
				MarkChanges(g);
				return count;
			}

			/// <summary>
			/// fix issues with node references if any exist
			/// </summary>
			public static bool FixNodeRefs(PVMenu_Graph g)
			{
				// load assets afresh
				var nodes = g.LoadNestedAssets<MenuNode>();
				// update node list
				if (!nodes.IsSameAs(g._nodes))
				{
					var so = new SerializedObject(g);

					// mark time of change
					so.FindProperty(MODIFIED_FIELD).intValue = GetTimestamp();

					var narr = so.FindProperty(NODES_FIELD);
					narr.arraySize = nodes.Length;
					for (var i = 0; i < nodes.Length; i++)
					{
						narr.GetArrayElementAtIndex(i).objectReferenceValue = nodes[i];
					}
					so.ApplyModifiedPropertiesWithoutUndo();
					return true;
				}
				return false;
			}

			/// <summary>
			/// Set modified timestamp and mark dirty
			/// </summary>
			private static void MarkChanges(PVMenu_Graph g)
			{
				g._lastModified = GetTimestamp();
				EditorUtility.SetDirty(g);
			}

			private static int GetTimestamp()
			{
				return (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
			}

		}
	}
}

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using UnityEditor;
	using UnityEngine;
	using System.Collections.Generic;
	using UnityEditorInternal;

	[CustomEditor(typeof(PVMenu_Graph))]
	internal sealed class _PVMenu_Graph : _Inspector
	{
		protected override void OnAfterFields()
		{
#if SM_DEV || SM_DEBUG
			GUILayout.Space(5f);
			DrawNodeList();
			DrawGraphActions();
#endif
		}

		private ReorderableList _nodes;
		private PVMenu_Graph _target;
		private bool _foldout;

		private readonly Dictionary<string, System.Action<PVMenu_Graph>> _GRAPH_ACTIONS = new()
		{
			{ "Fix References", FixNodeRefs },
			{ "Clear Invalid Edges", ClearInvalidEdges },
		};

		protected override void OnEnable()
		{
			base.OnEnable();
			
			_target = (PVMenu_Graph)target;

			var nodes = serializedObject.FindProperty(PVMenu_Graph.WithUndo.NODES_FIELD);
			_nodes = new (serializedObject, nodes, false, false, false, false)
			{
				headerHeight = 0f
			};

			_nodes.drawElementCallback = (pos, i, a, f) =>
			{
				var n = _target.GetNodeAt(i);
				if (!n)
				{
					EditorGUI.DrawRect(pos, Color.red);
					EditorGUI.LabelField(pos, "<missing>");
					return;
				}
				EditorGUI.LabelField(pos, n.Label);
			};
		}

		private void DrawGraphActions()
		{
			if (_GRAPH_ACTIONS.Count == 0)
			{
				return;
			}
			GUILayout.BeginHorizontal();
			foreach (var item in _GRAPH_ACTIONS)
			{
				if (GUILayout.Button(item.Key))
				{
					item.Value.Invoke(_target); 
				}
			}
			GUILayout.EndHorizontal();
		}

		private void DrawNodeList()
		{
			GUILayout.BeginVertical(GUI.skin.box);
			EditorGUI.indentLevel++;
			_foldout = EditorGUILayout.Foldout(_foldout, _nodes.serializedProperty.displayName, true);
			EditorGUI.indentLevel--;
			if (_foldout)
			{
				GUILayout.Space(5f);
				_nodes.DoLayoutList();
			}
			GUILayout.EndVertical();
		}

		private static void ClearInvalidEdges(PVMenu_Graph g)
		{
			PVMenu_Graph.WithUndo.DeleteInvalidEdges(g);
		}

		private static void FixNodeRefs(PVMenu_Graph g)
		{
			PVMenu_Graph.WithUndo.FixNodeRefs(g);
		}
	}
}