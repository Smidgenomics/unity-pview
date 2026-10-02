// smidgens @ github

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using System;
	using UnityEngine;

	/// <summary>
	/// Override field label width
	/// </summary>
	[AttributeUsage(AttributeTargets.Field)]
	internal sealed class FieldLabelWidthAttribute : PropertyAttribute
	{
		internal float width { get; }

		public FieldLabelWidthAttribute(float width)
		{
			this.width = width;
		}
	}
}