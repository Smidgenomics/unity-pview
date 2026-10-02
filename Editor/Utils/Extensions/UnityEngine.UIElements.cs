// smidgens @ github

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using UnityEngine;
	using UnityEngine.UIElements;

	/// <summary>
	/// UI Toolkit helpers
	/// </summary>
	internal static class UIElements_
	{
		// add multiple classes at once to element
		public static void AddClasses(this VisualElement element, params string[] classes)
		{
			foreach (var cls in classes)
			{
				element.AddToClassList(cls);
			}
		}

		// add/remove class based on state
		public static void SetClassActive(this VisualElement el, string className, bool active)
		{
			if (active)
			{
				el.AddToClassList(className);
			}
			else
			{
				el.RemoveFromClassList(className);
			}
		}

		
		public static T AddChildWithClass<T>(this VisualElement el, string elName, string className) where T : VisualElement, new()
		{
			var cEl = new T
			{
				name = elName
			};
			cEl.AddToClassList(className);
			el.Add(cEl);
			return cEl;
		}

		// shorthand for setting both x and y components in pixels
		public static void SetBackgroundPositionInPixels(this IStyle style, in Vector2 pos)
		{
			style.backgroundPositionX =
				new StyleBackgroundPosition(new BackgroundPosition(BackgroundPositionKeyword.Left, pos.x));
			style.backgroundPositionY =
				new StyleBackgroundPosition(new BackgroundPosition(BackgroundPositionKeyword.Top, pos.y));
		}

		// shorthand for setting left/top values
		public static void SetPosition(this IStyle style, in Vector2 pos)
		{
			style.left = pos.x;
			style.top = pos.y;
		}

		public static void TranslatePosition(this IStyle style, in Vector2 delta)
		{
			var x = style.left.value.value;
			var y = style.left.value.value;
			style.left = x + delta.x;
			style.top = y + delta.y;
		}
	}
}
