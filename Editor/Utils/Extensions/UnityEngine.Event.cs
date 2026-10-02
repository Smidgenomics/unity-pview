// smidgens @ github

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using UnityEngine;

	/// <summary>
	/// Extensions for UnityEngine.Event
	/// </summary>
	internal static class Event_
	{
		/// <summary>
		/// Context click
		/// </summary>
		public static bool IsContextClick(this Event e)
		{
			return
			e != null
			&& Event.current.type == EventType.ContextClick;
		}

		/// <summary>
		/// Is mouse up
		/// </summary>
		public static bool IsMouseUp(this Event e, int b) => IsButton(e, EventType.MouseUp, b);

		private static bool IsButton(Event e, EventType t, int button)
		{
			return
			e != null
			&& e.type == t
			&& e.button == button;
		}
	}
}
