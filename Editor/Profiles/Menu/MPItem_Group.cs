// smidgens @ github

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using System;
	using Newtonsoft.Json;
	using UnityEngine;

	[TypeAlias("group")]
	internal sealed class MPItem_Group : PVMenuProfileItem
	{
		[JsonProperty("items")]
		public PVMenuProfileItem[] children { get; internal set; } = Array.Empty<PVMenuProfileItem>();

		protected override void OnDeserialized()
		{
			children ??= Array.Empty<PVMenuProfileItem>();
		}

		public override void PopulateMenu(string path, MenuGenContext ctx)
		{
			if (path != string.Empty)
			{
				path += "/";
			}
			if (!string.IsNullOrEmpty(label))
			{
				path += label;
			}
			foreach (var c in children)
			{
				if (c == null)
				{
					ctx.AddDivider(path);
				}
				else
				{
					c.PopulateMenu(path, ctx);
				}
			}
		}
	}
}