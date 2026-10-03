// smidgens @ github

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using UnityEngine;
	using UnityEditor;
	using System;
	using System.Reflection;
	using Object = UnityEngine.Object;

	/// <summary>
	/// Wrappers for misc niche logic
	/// </summary>
	internal static class UnityUtility
	{
		// Retrieve System.Type for main asset at given path
		public static Type GetTypeAtPath(string assetPath) => AssetDatabase.GetMainAssetTypeAtPath(assetPath);

		// Executes menu item path
		public static void ExecuteMenu(string menuItemPath) => EditorApplication.ExecuteMenuItem(menuItemPath);

		/// <summary>
		/// Check if given menu path can be executed in current context
		/// For example: can an asset be created in the current location
		/// </summary>
		public static bool CanExecuteMenu(string menuItemPath) => Menu.GetEnabled(menuItemPath);

		// Returns list of menu items at given root path (context menu, window etc.)
		public static string[] GetSubmenus(string menuPath, bool separators = false)
		{
			return separators
			? Unsupported.GetSubmenusIncludingSeparators(menuPath)
			: Unsupported.GetSubmenus(menuPath);
		}

		public static EditorWindow GetProjectWindow()
		{
			var t = Type.GetType("UnityEditor.ProjectBrowser, UnityEditor.CoreModule");
			return EditorWindow.GetWindow(t);
		}
		
		// hack for advanced dropdown popups auto expanding to ridiculous sizes
		public static void SetLastAdvancedDropdownHeight(Rect rect, float maxHeight, float maxWidth = 0f)
		{
			var window = EditorWindow.focusedWindow;

			if(!window || window.GetType().Name != "AdvancedDropdownWindow")
			{
				return;
			}

			var position = window.position;

			position.height = Mathf.Min(maxHeight, position.height);
			
			if (!Mathf.Approximately(0f, maxWidth))
			{
				position.width = Mathf.Min(maxWidth, position.width);
			}
			window.minSize = position.size;
			window.maxSize = position.size;
			window.position = position;
			window.ShowAsDropDown(GUIUtility.GUIToScreenRect(rect), position.size);
		}

		// return a usable mouse coordinate in project view
		public static Vector2 GetContextPosInProjectView(Event ev)
		{
			// WIP: not figured this one out yet...
			// The project browser is a weird animal
			var pWin = UnityUtility.GetProjectWindow();
			GetProjectBrowserField("m_ListAreaRect", ref _listAreaRectField);
			GetProjectBrowserField("m_TreeViewRect", ref _treeAreaRect);
			return default;

		}

		// note: only SOs supported atm
		public static void StartCreatingAsset(Type t)
		{
			if (t.IsAbstract || !typeof(Object).IsAssignableFrom(t))
			{
				return;
			}
			if (typeof(ScriptableObject).IsAssignableFrom(t))
			{
				var so = ScriptableObject.CreateInstance(t);
				ProjectWindowUtil.CreateAsset(so, $"New {ObjectNames.NicifyVariableName(t.Name)}.asset");
			}
			else
			{
				if (t.GetConstructor(Type.EmptyTypes) == null)
				{
					return;
				}
				var asset = (Object)Activator.CreateInstance(t);
				ProjectWindowUtil.CreateAsset(asset, $"New {ObjectNames.NicifyVariableName(t.Name)}.asset");
			}
		}

		private static FieldInfo GetProjectBrowserField(string fName, ref FieldInfo cache)
		{
			if (cache != null)
			{
				return cache;
			}
			var t = Type.GetType("UnityEditor.ProjectBrowser, UnityEditor.CoreModule")!;
			cache = t.GetField(fName, BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic);
			return cache;
		}
		
		// m_ListAreaRect
		private static FieldInfo _listAreaRectField;
		
		// m_TreeViewRect
		private static FieldInfo _treeAreaRect;
	}
}