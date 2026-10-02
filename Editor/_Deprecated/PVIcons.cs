// smidgens @ github

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using UnityEngine;

	internal abstract class PVIcons : ScriptableObject
	{
		public virtual void Draw(string guid, in Rect pos)
		{
		}
	}
}