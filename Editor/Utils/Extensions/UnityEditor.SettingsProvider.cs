// smidgens @ github

#pragma warning disable 0414

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using System.Reflection;
	using UnityEditor;

	internal static class SettingsProvider_
	{
		// annoying workaround because setting scope is internal
		public static void SetScope(this SettingsProvider provider, SettingsScope scope)
		{
			var ptype = typeof(SettingsProvider);
			var bf = ptype.GetField("<scope>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.NonPublic);
			bf!.SetValue(provider, scope);
		}
	}
}