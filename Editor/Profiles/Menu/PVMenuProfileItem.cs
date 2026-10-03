// smidgens @ github

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using System.Runtime.Serialization;
	using Newtonsoft.Json;
	
	// base class for menu item types
	public abstract class PVMenuProfileItem
	{
		// note: not used yet as UnityEditor.GenericMenu (used by current impl) doesn't support icons
		[JsonProperty("icon")] public string iconGUID { get; private set; }

		[JsonProperty("label")] public string label { get; private set; }

		[OnDeserialized]
		private void OnDeserialized(StreamingContext context) => OnDeserialized();

		// called after values have been deserialized
		protected virtual void OnDeserialized() {}

		// allow node to populate item
		public virtual void PopulateMenu(string path, MenuGenContext context)
		{
			// add options here
		}
	}
	
	
}