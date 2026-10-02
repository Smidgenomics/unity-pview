// smidgens @ github

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using UnityEngine;

	/// <summary>
	/// Base class for node type in menu graph
	/// </summary>
	internal abstract class MenuNode : ScriptableObject
	{
		public string Label => GetLabel();
		public float SortWeight => _position.y;
		public Vector2 Position => _position;
		public int Inputs => GetInputCount();
		public int Outputs => GetOutputCount();
		public Color Color => GetColor();

		public virtual System.Action GetInvokeHandler()
		{
			return null;
		}

		/// <summary>
		/// Save with undo
		/// </summary>
		public static class WithUndo
		{
			public static void SetPosition(MenuNode n, Vector2 v)
			{
				UnityEditor.Undo.RecordObject(n, "Set menu node position");
				n._position = v;
				UnityEditor.EditorUtility.SetDirty(n);
			}
		}

		protected virtual string GetLabel()
		{
			if (string.IsNullOrEmpty(_label))
			{
				return "(untitled)";
			}
			return _label;
		}

		protected virtual Color GetColor() => Color.white;
		protected virtual int GetOutputCount() => 0;
		protected virtual int GetInputCount() => 1;
		public virtual Texture GetIcon() => null;

		[SerializeField] protected string _label = string.Empty;

		// position in graph
		[HideInInspector]
		[SerializeField] protected Vector2 _position;

	}
}

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using UnityEditor;

	[CustomEditor(typeof(MenuNode), true)]
	internal sealed class _MenuNode : _Inspector
	{
		public override bool RequiresConstantRepaint() => true;
	}

}