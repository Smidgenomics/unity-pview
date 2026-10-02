// smidgens @ github

#pragma warning disable 0414

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using UnityEngine;
	using UnityEngine.UIElements;
	using System;
	using UnityEditor;

	/// <summary>
	/// Move element on mouse drag
	/// </summary>
	internal sealed class MoveManipulator : PointerManipulator
	{
		public MoveManipulator(Action<Vector2> onMoved)
		{
			_pointerId = -1;
			activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse });
			_active = false;
			_onMoved = onMoved;
		}

		private Vector2 _totalDelta;
		private Vector3 _start;
		private Vector3 _lastPos;
		private bool _active;
		private int _pointerId;
		private Vector2 _startSize;
		private readonly Action<Vector2> _onMoved;

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

			if (CanStartManipulation(e))
			{
				_totalDelta = default;
				_lastPos = e.localPosition;
				_start = e.localPosition;
				_pointerId = e.pointerId;
				_active = true;
				target.CapturePointer(_pointerId);
				e.StopPropagation();
			}
		}

		private void OnPointerMove(PointerMoveEvent e)
		{
			if (!_active || !target.HasPointerCapture(_pointerId))
			{
				return;
			}

			Vector2 diff = e.localPosition - _start;
			// Vector2 diff = _lastPos - _start;
			_lastPos = e.localPosition;

			target.style.top = target.layout.y + diff.y;
			target.style.left = target.layout.x + diff.x;

			_totalDelta += diff;
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
			_onMoved?.Invoke(_totalDelta);
		}
	}
}