// smidgens @ github


/*
 * GUIScope defines various disposables to wrap
 * common imgui-based setup/cleanup operations
 */

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using UnityEditor;
	using System;

	// disposables
	internal static partial class GUIScope
	{
		/// <summary>
		/// EditorWindow.BeginWindows/EndWindows
		/// </summary>
		public struct Windows : IDisposable
		{
			public Windows(EditorWindow window)
			{
				_window = window;
				_window.BeginWindows();
			}

			public void Dispose()
			{
				Dispose(disposing: true);
				GC.SuppressFinalize(this);
			}

			private void Dispose(bool disposing)
			{
				if (!_window) { return; }
				if (!disposing) { return; }
				_window.EndWindows();
				_window = null;
			}
			private EditorWindow _window;
		}
	}
}


namespace Smidgenomics.Unity.ProjectView.Editor
{
	using UnityEngine;
	using System;

	internal static partial class GUIScope
	{
		/// <summary>
		/// Unclips default OnGUI clip
		/// Useful for setting up area for draggable windows
		/// 
		/// Note: does not properly take toolbar area into account
		/// </summary>
		public struct Unclip : IDisposable
		{
			public Unclip(Matrix4x4 oldMatrix)
			{
				_oldMatrix = oldMatrix;
				_disposed = false;
				Begin();
			}

			public void Dispose()
			{
				Dispose(disposing: true);
				GC.SuppressFinalize(this);
			}

			private const float W_TAB_HEIGHT = 21f;
			private bool _disposed;
			private Matrix4x4 _oldMatrix;

			private void Dispose(bool disposing)
			{
				if (_disposed) { return; }
				if (!disposing) { return; }
				_disposed = true;
				End();
			}

			private void Begin()
			{
				GUI.EndGroup(); // end implicit clip group
			}

			private void End()
			{
				GUI.matrix = _oldMatrix;
				// restore implicit group
				GUI.BeginGroup(new Rect(0.0f, W_TAB_HEIGHT, Screen.width, Screen.height));
			}

		}
	}
}

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using UnityEngine;

	internal static partial class GUIScope
	{
		/// <summary>
		/// Clip screen rect
		/// </summary>
		public ref struct Clip
		{
			public Clip(in Rect pos)
			{
				_valid = true;
				_disposed = false;
				GUI.BeginClip(pos);
			}

			public void Dispose()
			{
				if (!_valid || _disposed) { return; }
				_disposed = true;
				GUI.EndClip();
			}
			private bool _valid, _disposed;
		}
	}
}