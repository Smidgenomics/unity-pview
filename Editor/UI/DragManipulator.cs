// smidgens @ github

#pragma warning disable 0414

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using UnityEngine;
	using UnityEngine.UIElements;
	using System;

	/// <summary>
	/// Handles mouse drag on element
	/// Note: Pulled from Unity docs and modified
	/// </summary>
	internal sealed class DragManipulator : PointerManipulator
	{
		public DragManipulator(MouseButton button, Action<Vector2> onDragDelta, Action<Vector2> onDragEnd = null)
		{
			_pointerId = -1;
			activators.Add(new ManipulatorActivationFilter { button = button });
			_active = false;
			_onDragged = onDragDelta;
			_onDragEnd = onDragEnd;
		}

		public event Action onDragStart;
		public Func<PointerDownEvent, bool> startPredicate;

		private Vector3 _start;
		private Vector3 _lastPos;
		private bool _active;
		private int _pointerId;
		private Vector2 _startSize;
		private readonly Action<Vector2> _onDragged;
		private readonly Action<Vector2> _onDragEnd;

		protected override void RegisterCallbacksOnTarget()
		{
			target.RegisterCallback<PointerDownEvent>(OnPointerDown);
			target.RegisterCallback<PointerMoveEvent>(OnPointerMove);
			target.RegisterCallback<PointerUpEvent>(OnPointerUp);
		}

		protected override void UnregisterCallbacksFromTarget()
		{
			target.UnregisterCallback<PointerDownEvent>(OnPointerDown);
			target.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
			target.UnregisterCallback<PointerUpEvent>(OnPointerUp);
		}

		private void OnPointerDown(PointerDownEvent e)
		{
			if (_active)
			{
				e.StopImmediatePropagation();
				return;
			}

			if (startPredicate != null && !startPredicate.Invoke(e))
			{
				return;
			}

			if (CanStartManipulation(e))
			{
				_start = e.localPosition;
				_lastPos = e.localPosition;
				_pointerId = e.pointerId;
				_active = true;
				target.CapturePointer(_pointerId);
				e.StopPropagation();
				onDragStart?.Invoke();
			}
		}

		private void OnPointerMove(PointerMoveEvent e)
		{
			if (!_active || !target.HasPointerCapture(_pointerId))
			{
				return;
			}
			
			_onDragged?.Invoke(e.localPosition - _lastPos);
			_lastPos = e.localPosition;
			e.StopPropagation();
		}

		private void OnPointerUp(PointerUpEvent e)
		{
			if (!_active || !target.HasPointerCapture(_pointerId) || !CanStopManipulation(e))
			{
				return;
			}
			_active = false;
			target.ReleaseMouse();
			e.StopPropagation();

			_onDragEnd?.Invoke(_lastPos - _start);
			
		}
	}
}