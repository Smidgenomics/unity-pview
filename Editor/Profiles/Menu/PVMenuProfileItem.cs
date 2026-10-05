// smidgens @ github

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using System.Reflection;
	using System.Runtime.Serialization;
	using Newtonsoft.Json;

	// base class for menu item types
	public abstract class PVMenuProfileItem
	{
		// note: not used yet as UnityEditor.GenericMenu (used by current impl) doesn't support icons
		[InjectVariables]
		[JsonProperty("icon")] private string _iconGUID;

		[field:InjectVariables]
		[JsonProperty("label")] public string label { get; private set; }

		[OnDeserialized]
		private void OnDeserialized(StreamingContext context) => OnDeserialized();

		// called after values have been deserialized
		protected virtual void OnDeserialized() {}

		// initializes
		public virtual void OnInit(MenuGenContext context)
		{
			context.InjectVariables(this);
		}

		// allow node to populate item
		public virtual void PopulateMenu(string path, MenuGenContext context)
		{
			// add options here
		}
	}
	
	
}