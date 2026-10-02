// smidgens @ github

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using UnityEngine;
	using UnityEngine.Events;
	using System;
	using System.ComponentModel;

	/// <summary>
	/// Menu option node
	/// </summary>
	[DisplayName("Action")]
	internal sealed class MenuAction : MenuNode
	{
		public static readonly Color NODE_COLOR = new (0.925f, 0.474f, 0.074f);

		public override Action GetInvokeHandler()
		{
			if (_handler == null || !_handler.IsEnabled())
			{
				return null;
			}
			return Invoke;
		}

		public void Invoke()
		{
			_handler?.Invoke();
			// _onAfterInvoke.Invoke();
		}

		protected override Color GetColor() => NODE_COLOR;

		protected override string GetLabel()
		{
			if (!string.IsNullOrEmpty(_label))
			{
				return _label;
			}
			var l = _handler?.GetLabel();
			return string.IsNullOrEmpty(l) ? base.GetLabel() : l;
		}

		private enum ActionType
		{
			ExecuteMenuItem, InvokeUnityEvent,
		}

		[SerializeReference]
		[InstancedReference(indent:false)]
		[SerializeField] private ActionHandler _handler = new ActionHandler_MenuItem();

		[Space]
		[Header("Deprecated")]
		[Obsolete]
		[SerializeField] private ActionType _type;
		// menu option
		[Inline]
		[Obsolete]
		[SerializeField] private AssetMenuItemRef _menuItem;

		[Obsolete]
		[SerializeField] private UnityEvent _onEvent;

		[Obsolete]
		[SerializeField] private UnityEvent _onAfterInvoke;

		[Serializable]
		internal abstract class ActionHandler
		{
			public void Invoke()
			{
				OnInvoke();
			}

			protected abstract void OnInvoke();

			public virtual string GetLabel() => string.Empty;

			public virtual bool IsEnabled()
			{
				return true;
			}
		}

		// invoke menu item
		[DisplayName("Menu Item")]
		[Serializable]
		private sealed class ActionHandler_MenuItem : ActionHandler
		{
			[FieldLabel(null)]
			[EditorMenuItem("Assets")]
			[SerializeField] private string _path;

			public override bool IsEnabled()
			{
				return !string.IsNullOrEmpty(_path)
				&& UnityUtility.CanExecuteMenu(_path);
			}

			public override string GetLabel()
			{
				if (!string.IsNullOrEmpty(_path))
				{
					var sIndex = Mathf.Max(0, _path.LastIndexOf('/') + 1);
					return _path.Substring(sIndex);
				}
				return string.Empty;
			}

			protected override void OnInvoke()
			{
				UnityUtility.ExecuteMenu(_path);
			}
		}

		// invoke unity event
		[DisplayName("Unity Event")]
		[Serializable]
		private sealed class ActionHandler_Event : ActionHandler
		{
			[SerializeField] private UnityEvent _event;

			public override bool IsEnabled()
			{
				return _event.GetPersistentEventCount() > 0;
			}
			protected override void OnInvoke()
			{
				_event.Invoke();
			}
		}
	}
}