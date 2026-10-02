// smidgens @ github

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using UnityEngine;
	using System;

	// wrapper around referencing and executing menu path
	[Serializable]
	internal struct AssetMenuItemRef
	{
		public readonly void Invoke()
		{
			if (!CanExecuteItem())
			{
				return;
			}
			UnityUtility.ExecuteMenu(_item);
		}

		public readonly bool CanExecuteItem()
		{
			return !string.IsNullOrEmpty(_item)
			&& UnityUtility.CanExecuteMenu(_item);
		}

		[EditorMenuItem("Assets")]
		[SerializeField] private string _item;
	}
}