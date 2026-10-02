// smidgens @ github

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using UnityEngine;

	/// <summary>
	/// 
	/// </summary>
	internal enum EOverrideBehaviour
	{
		/// <summary>
		/// Vanilla Unity
		/// </summary>
		[InspectorName("Unity")] UnityDefault,
		/// <summary>
		/// Use profile in project settings
		/// </summary>
		[InspectorName("Project")] ProjectProfile,
		/// <summary>
		/// Use profile from user settings
		/// </summary>
		[InspectorName("User")] UserProfile
	}
}

namespace Smidgenomics.Unity.ProjectView.Editor
{
	/// <summary>
	/// Scene or project browser
	/// </summary>
	internal enum EOverrideScope
	{
		/// <summary>
		/// Project browser
		/// </summary>
		Project,
		/// <summary>
		/// Scene hierarchy
		/// </summary>
		Scene
	}
}