// smidgens @ github

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using System;
	using Newtonsoft.Json;

	/// <summary>
	/// Item that contains only child items
	/// </summary>
	[TypeAlias("group")]
	internal sealed class MPItem_Group : PVMenuProfileItem
	{
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
			foreach (var c in _children)
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

		public override void OnInit(MenuGenContext context)
		{
			base.OnInit(context);
			foreach (var c in _children)
			{
				c?.OnInit(context);
			}
		}

		protected override void OnDeserialized()
		{
			_children ??= Array.Empty<PVMenuProfileItem>();
		}

		[JsonProperty("items")] private PVMenuProfileItem[] _children { get; set; }
	}
}