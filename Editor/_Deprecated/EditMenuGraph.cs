// smidgens @ github

#pragma warning disable CS0618 // Type or member is obsolete
namespace Smidgenomics.Unity.ProjectView.Editor
{
	using UnityEngine;
	using UnityEditor;
	using System;
	using System.ComponentModel;
	using System.Reflection;

	internal sealed class EditMenuGraph : EditorWindow
	{
		public const string WINDOW_TITLE = "Edit Menu (Graph)";
		public const string WINDOW_TITLE_DIRTY = WINDOW_TITLE + "*";
		public const float LINE_THICKNESS = 3f;
		public const float ZOOM_MIN = 0.25f;
		public const float ZOOM_MAX = 2f;
		public const float ZOOM_STEP = 0.125f;
		public const float ZOOM_DEFAULT = 1f;
		public const float DELETE_BTN_SIZE = 13f;
		public const float GRID_SNAP = 10;
		public static readonly Vector2 NODE_SIZE = new (100f, 20f);
		public const float PORT_SIZE = 10f;
		public const float PORT_OFFSET = 2f;
		public const float NODE_COLOR_WIDTH = 3f;
		private const float _INSPECTOR_WIDTH = 300f;

		private static Lazy<Texture> GetLazyTex(string name)
		{
			return new (() => EditorGUIUtility.IconContent(name)?.image);
		}

		public static readonly Lazy<Texture> _ICO_CIRCLE = GetLazyTex("AvatarInspector/DotFrame");
		public static readonly Lazy<Texture> _ICO_CIRCLE_DOTTED = GetLazyTex("AvatarInspector/DotFrameDotted");
		public static readonly Lazy<Texture> _ICO_DOT_YELLOW = GetLazyTex("sv_icon_dot4_pix16_gizmo");
		public static readonly Lazy<Texture> _ICO_DELETE = GetLazyTex("P4_DeletedRemote");
		public static readonly Lazy<Texture> _ICO_ROOT_NODE = GetLazyTex("Favorite Icon");

		public static void Open(PVMenu_Graph p)
		{
			var w = GetWindow<EditMenuGraph>(typeof(SceneView));
			w._Graph = p;
			w.Show();
		}

		private PVMenu_Graph _Graph
		{
			get => _graph;
			set
			{
				if (value == _graph) { return; }
				_graph = value;
				ResetLayout();
			}
		}

		[SerializeField] private PVMenu_Graph _graph;

		// node layout rects
		private Rect[] _nodeRects = Array.Empty<Rect>();
		private Connector _connector;
		private readonly ViewportTransform _transform = new ();
		private MenuNode _pressedNode;
		private Tuple<int, MenuNode> _draggedNode;
		private Editor _currentInspector;
		private Texture _gridTex;

		private sealed class Connector
		{
			public MenuNode node;
			public int port;
			public int index;
		}

		// transformation per user movement (pan, zoom)
		private sealed class ViewportTransform
		{
			public Vector2 drag { get; set; }
			public float scale { get; private set; } = ZOOM_DEFAULT;

			public Matrix4x4 GetMatrix()
			{
				var s = Vector3.one * scale;
				return Matrix4x4.TRS(drag, Quaternion.identity, s);
			}

			public void Zoom(int sign)
			{
				var z = scale + (-sign * ZOOM_STEP);

				var oldScale = scale;
				scale = Mathf.Clamp(z, ZOOM_MIN, ZOOM_MAX);

				var drag1 = oldScale * drag;
				var drag2 = scale * drag;

				var diff = drag2 - drag1;
				drag += diff;

				// var offset = -drag * scale;
				// drag += offset;
			}

			public void Reset()
			{
				drag = Vector2.zero;
				scale = ZOOM_DEFAULT;
			}
		}

		// Known editor asset guids
		private const string GUID_TEX_GRID = "f92e99d0b72c60d49b99fab431718d29";
		
		private void OnEnable()
		{
			titleContent.text = WINDOW_TITLE;
			Undo.undoRedoPerformed -= OnUndo;
			Undo.undoRedoPerformed += OnUndo;
			var gridPath = AssetDatabase.GUIDToAssetPath(GUID_TEX_GRID);
			_gridTex = AssetDatabase.LoadAssetAtPath<Texture2D>(gridPath);
			ResetLayout();
		}

		private void OnDisable()
		{
			_draggedNode = null;
			Undo.undoRedoPerformed -= OnUndo;
			_nodeRects = Array.Empty<Rect>();
		}

		private void OnUndo()
		{
			_nodeRects = Array.Empty<Rect>();
			EditorApplication.delayCall += Repaint;
		}

		public void Update()
		{
			if (NeedsRepaint())
			{
				Repaint();
			}
		}

		private Rect GetCanvasRect()
		{
			var r = position;
			r.position = default;
			return r;
		}

		// TODO (plz): clean up this horribleness
		private void DrawBackground(in Rect canvasRect)
		{
			var z = (_transform.scale - ZOOM_MIN) / (ZOOM_MAX - ZOOM_MIN);
			var opacity = Mathf.Lerp(0.1f, 0.2f, z);
			var bgScale = Mathf.Lerp(ZOOM_MAX, ZOOM_MIN, z);
			DrawTiledBackground(canvasRect, _gridTex, bgScale, _transform.drag, Color.black * opacity);
		}

		private bool _insideInspector;

		private void OnGUI()
		{
			_insideInspector = false;
			if (!_Graph)
			{
				return;
			}

			// refresh
			RefreshTitle();
			// init
			ComputeLayoutRects();
			// pre-events
			ProcessCanvasDrag(Event.current);
			ProcessZoomEvents(Event.current);
			ProcessNodeDrag(Event.current);

			var canvasRect = GetCanvasRect();

			DrawBackground(canvasRect);

			var inspectorRect = _currentInspector
			? canvasRect.SliceRight(_INSPECTOR_WIDTH)
			: default;

			_insideInspector = _currentInspector && inspectorRect.Contains(Event.current.mousePosition);

			using (new GUIScope.Unclip(GUI.matrix))
			{
				// scale etc
				ApplyTransformMatrix();

				DrawEdges();

				using (new GUIScope.Windows(this))
				{
					DrawNodes();
				}
				DrawConnector();
				ProcessBackgroundEvents(Event.current);
			}

			DrawOverlayWidgets();
			
			if (_currentInspector)
			{
				DrawSelectionInspector(inspectorRect);
			}
			
			// Repaint();
		}

		private void OnInspectorUpdate()
		{
			Repaint();
		}

		private void DrawOverlayWidgets()
		{
			var area = position;
			area.position = Vector2.zero;
		}

		private void DrawSelectionInspector(in Rect area)
		{
			EditorGUI.DrawRect(area, UnityUtility.EditorBackgroundTint);

			var borderLeft = area;
			borderLeft.width = 1f;
			EditorGUI.DrawRect(borderLeft, Color.black * 0.5f);

			var inner = area;
			inner.Resize(-4f);
			
			GUILayout.BeginArea(inner);
			
			_currentInspector.OnInspectorGUI();
			
			GUILayout.EndArea();
			
		}
		
		public static void DrawTiledBackground(in Rect r, Texture tex, float scale, Vector2 offset, Color color)
		{
			GUI.BeginClip(r);
		
			var bgRect = r;
			bgRect.position = default;
			bgRect.height = bgRect.width;

			var bgWidth = bgRect.width * scale;

			var cellWidth = 50f;

			//var repeat = 2f;
			var repeat = bgWidth / cellWidth;

			var coords = new Rect(0f, 0f, repeat, repeat);

			var dragScaleX = -(offset.x / bgRect.width) * repeat;
			var dragScaleY = (offset.y / bgRect.width) * repeat;

			var cpos = new Vector2(dragScaleX, dragScaleY);
			//coords.position = cpos + Vector2.one * constOffset;
			coords.position = cpos;

			var tColor = GUI.color;
			GUI.color = color;
			Graphics.DrawTexture(bgRect, tex, coords, 0, 0, 0, 0, color);
			GUI.color = tColor;
		
			GUI.EndClip();
		}

		/// <summary>
		/// Clear layout state
		/// </summary>
		private void ResetLayout()
		{
			_nodeRects = Array.Empty<Rect>();
			_transform.Reset();
			CenterOnNodes();
		}

		/// <summary>
		/// 
		/// </summary>
		private void RefreshTitle()
		{
			var modified = IsModified();
			titleContent.text = modified ? WINDOW_TITLE_DIRTY : WINDOW_TITLE;
		}

		private bool IsModified()
		{
			if (!_graph)
			{
				return false;
			}

			if (EditorUtility.IsDirty(_graph))
			{
				return true;
			}

			for (var i = 0; i < _graph.NodeCount; i++)
			{
				var n = _graph.GetNodeAt(i);
				if (!n)
				{
					continue;
				}

				if (EditorUtility.IsDirty(n))
				{
					return true;
				}

			}
			return false;
		}

		private void Select(MenuNode node)
		{
			// Selection.activeObject = node;
			if (_currentInspector)
			{
				DestroyImmediate(_currentInspector);
				_currentInspector = null;
			}
			
			if (node)
			{
				if (node.GetType() != typeof(MenuSeparator))
				{
					_currentInspector = Editor.CreateEditor(node);
				}
			}
		}

		private void CenterOnNodes()
		{
			_transform.drag = GetPositionOfNodes();
		}

		private Vector2 GetPositionOfNodes()
		{
			if (!_graph || _graph.NodeCount == 0)
			{
				return Vector2.zero;
			}
			var center = position.size * 0.5f;
			var nodePos = Vector2.zero;
			var nodeOffset = NODE_SIZE * 0.5f;
			// node to center on
			var node = _graph.RootNode ?? _graph.GetLeftMostNode();
			if (node) { nodePos = -node.Position; }
			return center + nodePos - nodeOffset;
		}

		private void ProcessNodeDrag(Event e)
		{
			
			if (_draggedNode == null) { return; }

			var index = _draggedNode.Item1;

			// apply if...
			var applyChanges =
			e.IsMouseUp(0) // mouse released
			|| !this.IsCurrentlyMoused(); // canvas lost focus

			if (applyChanges)
			{
				var i = index;
				//_draggedIndex = -1;
				_draggedNode = null;
				ApplyPositionChangeAt(i);
			}
		}

		private void ProcessCanvasDrag(Event e)
		{
			// mmb needs to be held
			if (e.button != 2)
			{
				return;
			}

			// should add drag
			var isDragging = e.type
			is EventType.MouseDrag
			or EventType.MouseDown;

			// nothing
			if (!isDragging)
			{
				return;
			}

			// apply drag to canvas
			e.Use();
			_transform.drag += e.delta;
		}

		private void ProcessZoomEvents(Event e)
		{
			if (!e.isScrollWheel)
			{
				return;
			}

			if (Mathf.Approximately(0, e.delta.y))
			{
				return;
			}
			e.Use();
			_transform.Zoom(sign: e.delta.y > 0 ? 1 : -1);
		}

		private void ProcessBackgroundEvents(Event e)
		{
			// is currently dragging node
			if (_insideInspector || _draggedNode != null)
			{
				return;
			}

			// show dropdown for adding nodes
			var showAddContext =
			_connector == null // shouldn't be connecting node
			&& e.type == EventType.ContextClick; // right clicked

			if (showAddContext)
			{
				e.Use();
				GetContextMenu().ShowAsContext();
			}

			// should selection be cleared?
			var clearSelection =
			e.type == EventType.MouseDown
			&& e.button != 2;

			if (clearSelection)
			{
				e.Use();
				ClearConnector();
				// Selection.activeObject = null;
				Select(null);
			}
		}

		

		private GenericMenu GetContextMenu()
		{
			var m = new GenericMenu();
			var e = Event.current;
			// add node at position
			var pos = e.mousePosition;
			m.AddDisabledItem(new GUIContent("Add"));
			m.AddSeparator(string.Empty);

			foreach (var nType in TypeCache.GetTypesDerivedFrom(typeof(MenuNode)))
			{
				var label = nType.Name;
				var dnAttr = nType.GetCustomAttribute<DisplayNameAttribute>();
				if (dnAttr != null)
				{
					label = dnAttr.DisplayName;
				}
				
				m.AddItem(new GUIContent(label), false, () =>
				{
					PVMenu_Graph.WithUndo.AddNode(_graph, nType, pos);
				});
			}
			return m;
		}

		private void OnPortClicked(MenuNode n, int port)
		{
			if (port == 0) { return; } // interpret as empty

			if (_connector == null)
			{
				_connector = new Connector
				{
					node = n,
					port = port,
					index = _graph.IndexOfNode(n)
				};
			}
			else
			{
				var sameType = Mathf.Approximately(Mathf.Sign(port), Mathf.Sign(_connector.port));
				if (sameType) { return; } // no homo
				ConnectNodes(_connector.node, n, _connector.port, port);
				ClearConnector();
			}
		}

		private void ConnectNodes(MenuNode n1, MenuNode n2, int p1, int p2)
		{
			// make sure output (<0) -> input (>0)
			if (p2 < p1)
			{
				Swap(ref n1, ref n2);
				Swap(ref p1, ref p2);
			}
			PVMenu_Graph.WithUndo.AddEdge(_graph, n1, n2);
		}

		private void DrawConnector()
		{
			// nothing is being dragged
			if (_connector == null)
			{
				return;
			}

			// compute 
			var nodeRect = _nodeRects[_connector.index];
			var p1 = ComputePortCenter(nodeRect, port: _connector.port);
			var p2 = Event.current.mousePosition;
			GraphGizmos.DrawDragConnector(p1, p2, Color.white);
		}

		// when returning true, the gui should be refreshed
		private bool NeedsRepaint()
		{
			return _connector != null;
		}

		private void ClearConnector() => _connector = null;

		private void ApplyTransformMatrix() => GUI.matrix = _transform.GetMatrix();

		private void ApplyPositionChangeAt(int i)
		{
			var node = _graph.GetNodeAt(i);
			if (!node)
			{
				return;
			}
			var pos = _nodeRects[i].position;
			MenuNode.WithUndo.SetPosition(node, pos);
		}

		private void ComputeLayoutRects()
		{
			if (_nodeRects.Length == _graph.NodeCount) { return; }
			_nodeRects = new Rect[_graph.NodeCount];
			for (var i = 0; i < _nodeRects.Length; i++)
			{
				var n = _graph.GetNodeAt(i);
				_nodeRects[i].size = NODE_SIZE;
				_nodeRects[i].position = n.Position;
			}
		}

		private void DrawNodes()
		{
			var c = _graph.NodeCount;
			try
			{
				for (var i = 0; i < c; i++)
				{
					DrawNodeAt(i);
				}
			}
			catch (Exception e)
			{
				Debug.Log(e.Message);
			}
		}

		private void DrawEdges()
		{
			try
			{
				var c = _graph.EdgeCount;
				for (var i = 0; i < c; i++)
				{
					DrawEdgeAt(i);
				}
			}
			catch
			{
				// ignored
			}
		}

		private void DrawNodeAt(int i)
		{
			var e = Event.current;
			if (e == null) { return; }

			var node = _graph.GetNodeAt(i);
			var nodeRect = _nodeRects[i];
			var oldPosition = nodeRect.position;

			DrawNodePorts(nodeRect, node);

			if (!_insideInspector)
			{
				ProcessNodeClickRight(nodeRect, e, node);
			}

			var wid = GUIUtility.GetControlID(FocusType.Passive);
			_nodeRects[i] = GUI.Window(wid, _nodeRects[i], _ =>
			{
				var draggable =
				!_insideInspector
				&& this.IsCurrentlyMoused()
				&& e.button == 0
				&& _connector == null;

				if (!_insideInspector)
				{
					ProcessNodeClick(e, node);
				}

				if (draggable)
				{
					GUI.DragWindow();
				}
				var innerArea = _nodeRects[i];
				innerArea.position = Vector2.zero;
				DrawNodeArea(node, innerArea);

				if (_graph.RootNode == node)
				{
					DrawRootNodeGUI(_nodeRects[i]);
				}
			}, string.Empty);

			var moved = !Mathf.Approximately(Vector2.Distance(_nodeRects[i].position, oldPosition), 0f);

			if (moved)
			{
				SnapRectToGrid(ref _nodeRects[i]);
				_draggedNode = new Tuple<int, MenuNode>(i, node);
			}
		}

		private void DrawRootNodeGUI(in Rect nodeRect)
		{
			var ico = _ICO_ROOT_NODE.Value;
			GraphGizmos.DrawIcon(nodeRect.size * 0.5f, ico, 20f);
		}

		private void DrawEdgeAt(int i)
		{
			var e = _graph.GetEdgeAt(i);
			if (e == null)
			{
				return;
			}

			if (!e.IsValid)
			{
				return;
			}

			var ni1 = _graph.IndexOfNode(e.N1);
			var ni2 = _graph.IndexOfNode(e.N2);
			if (ni1 < 0 || ni2 < 0)
			{
				return;
			}
			var nr1 = _nodeRects[ni1];
			var nr2 = _nodeRects[ni2];
			var p1 = ComputePortCenter(nr1, -1);
			var p2 = ComputePortCenter(nr2, 1);
			var mid = (p1 + p2) * 0.5f;
			GraphGizmos.DrawConnector(p1, p2, Color.white);

			if (DrawDeleteButton(mid))
			{
				DeleteEdgeAt(i);
			}
		}

		private static bool DrawDeleteButton(in Vector2 pos)
		{
			var rect = new Rect(0, 0, DELETE_BTN_SIZE, DELETE_BTN_SIZE);
			rect.center = pos;
			var v = GUI.Button(rect, "");
			GraphGizmos.DrawIcon(rect, _ICO_DELETE.Value);
			return v;
		}

		private void DeleteEdgeAt(int i)
		{
			PVMenu_Graph.WithUndo.DeleteEdge(_graph, i);
		}

		private static void SnapRectToGrid(ref Rect r)
		{
			r.position = r.position.SnapToGrid(GRID_SNAP);
		}

		private void DrawNodeArea(MenuNode n, in Rect r)
		{
			var isRoot = _graph.RootNode == n;
			var label = isRoot ? "" : n.Label;
			var color = isRoot ? Color.clear : n.Color;
			DrawNodeColor(r, color);
			DrawNodeLabel(r, label);
		}

		private void DrawNodeColor(in Rect nodeRect, in Color c)
		{
			var area = nodeRect;
			area.width = NODE_COLOR_WIDTH;
			var pos = area.position;
			area.position = pos;
			EditorGUI.DrawRect(area, c);
		}

		private void ProcessNodeClickRight(in Rect rect, Event e, MenuNode node)
		{
			if (e.type == EventType.ContextClick && rect.Contains(e.mousePosition))
			{
				e.Use();
				GetContextMenu(node).ShowAsContext();
			}
		}

		private void ProcessNodeClick(Event e, MenuNode node)
		{
			if (e.button != 0)
			{
				return;
			}

			if (e.type == EventType.MouseDown)
			{
				_pressedNode = node;
			}
			else if (e.type == EventType.MouseUp && _pressedNode == node)
			{
				_pressedNode = null;
				Select(node);
			}
		}

		private GenericMenu GetContextMenu(MenuNode node)
		{
			var m = new GenericMenu();
			if (_graph.RootNode != node)
			{
				m.AddItem(new GUIContent("Set Root"), false, () => SetRootMode(node));
				m.AddSeparator(string.Empty);
			}
			m.AddItem(new GUIContent("Delete"), false, () => DeleteNode(node));
			return m;
		}

		private void SetRootMode(MenuNode n)
		{
			PVMenu_Graph.WithUndo.SetRootNode(_graph, n);
		}

		private void DeleteNode(MenuNode n)
		{
			PVMenu_Graph.WithUndo.DeleteNode(_graph, n);
		}

		private static void DrawNodeLabel(in Rect r, string v)
		{

			if (v.Length > 10) { v = v.Substring(0, 10) + "..."; }

			var style = EditorStyles.boldLabel;
			var l = new GUIContent(v);
			var size = style.CalcSize(l);
			var pos = r.center;
			pos.x -= size.x * 0.5f;
			pos.y -= EditorGUIUtility.singleLineHeight * 0.5f;
			pos.y -= 2f;
			var area = r;
			area.position = pos;
			EditorGUI.LabelField(area, l, style);
		}

		private void DrawNodePorts(in Rect pos, MenuNode node)
		{
			var hasInputs = node.Inputs > 0 && _graph.RootNode != node;
			if (hasInputs)
			{
				for (var i = 0; i < node.Inputs; i++) { DrawNodePort(pos, node, i + 1); }
			}
			if (node.Outputs > 0)
			{
				for (var i = 0; i < node.Outputs; i++) { DrawNodePort(pos, node, -i - 1); }
			}
		}

		private void DrawNodePort(in Rect nodeRect, MenuNode n, in int port)
		{
			var center = ComputePortCenter(nodeRect, port);
			var box = GetCenteredBox(center, PORT_SIZE);
			var pressed = GUI.Button(box, GUIContent.none);

			GUI.DrawTexture(box, _ICO_CIRCLE_DOTTED.Value);

			if (pressed)
			{
				Event.current.Use();
				OnPortClicked(n, port);
			}
		}

		private Vector2 ComputePortCenter(in Rect area, in int port)
		{
			var flip = port < 0;
			var pos = area.center;
			var ox = (area.width * 0.5f) + PORT_OFFSET + (PORT_SIZE * 0.5f);
			var oy = 0;
			var offset = new Vector2(ox, oy);
			if (!flip) { offset.x *= -1f; }
			pos += offset;
			return pos;
		}

		private static Rect GetCenteredBox(in Vector2 center, in float size)
		{
			return new Rect(0, 0, size, size)
			{
				center = center
			};
		}

		private static void Swap<T>(ref T a, ref T b)
		{
			(a, b) = (b, a);
		}

		private static class GraphGizmos
		{
			public static void DrawIcon(in Vector2 pos, Texture t, float size)
			{
				GUI.DrawTexture(new Rect(0, 0, size, size)
				{
					center = pos
				}, t);
			}

			public static void DrawIcon(in Rect r, Texture t)
			{
				GUI.DrawTexture(r, t);
			}

			public static void DrawConnector(in Vector2 a, in Vector2 b, in Color c)
			{
				Handles.color = c;
				Handles.DrawAAPolyLine(LINE_THICKNESS, a, b);
				Handles.DrawLine(a, b);
				DrawIcon(a, _ICO_DOT_YELLOW.Value, 10f);
				DrawIcon(b, _ICO_DOT_YELLOW.Value, 10f);
			}

			public static void DrawDragConnector(in Vector2 a, in Vector2 b, in Color c)
			{
				Handles.color = c;
				Handles.DrawAAPolyLine(LINE_THICKNESS, a, b);
				DrawIcon(a, _ICO_DOT_YELLOW.Value, 8f);
			}
		}

	}


}