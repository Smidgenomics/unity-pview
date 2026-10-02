// smidgens @ github

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using UnityEngine;
	using UnityEditor;
	using System.Collections.Generic;

	/// <summary>
	/// Base type for custom editor menu
	/// </summary>
	internal abstract class PVMenu : ScriptableObject
	{
		public virtual GenericMenu GetMenu()
		{
			return new GenericMenu();
		}
	}
}