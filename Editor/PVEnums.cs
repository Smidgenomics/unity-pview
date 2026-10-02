// smidgens @ github

#pragma warning disable 0414

namespace Smidgenomics.Unity.ProjectView.Editor
{
	[System.Flags]
	internal enum EDefaultViewFlags
	{
		None = 0,
		Menu = 1,
		Icons = 2,
		All = ~0
	}
}

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using UnityEngine;

	internal enum EOverrideBehaviour
	{
		[InspectorName("Unity")] UnityDefault,
		[InspectorName("Project")] ProjectProfile,
		[InspectorName("User")] UserProfile
	}
}

namespace Smidgenomics.Unity.ProjectView.Editor
{
	internal enum EOverrideScope
	{
		Project,
		Scene
	}
}