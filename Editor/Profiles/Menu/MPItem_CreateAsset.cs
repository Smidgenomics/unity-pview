// smidgens @ github

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using System;
	using System.Reflection;
	using Newtonsoft.Json;
	using UnityEditor;
	using UnityEngine;

	// create asset(s)
	[TypeAlias("create")]
	internal sealed class MPItem_CreateAsset : PVMenuProfileItem
	{
		[JsonProperty] public string classType { get; internal set; } = string.Empty;
		
		// if true, will add a create option for every subtype
		[JsonProperty] public bool subClasses { get; internal set; }

		private Type _type;

		protected override void OnDeserialized()
		{
			_type = classType.IsGUID32()
			? GetMonoType(classType)
			: Type.GetType(classType);
		}

		public override void PopulateMenu(string path, MenuGenContext context)
		{
			if (_type == null)
			{
				return;
			}

			if (path.Length > 0)
			{
				path += "/";
			}

			if (!subClasses)
			{
				var lb = base.label;

				if (string.IsNullOrEmpty(lb))
				{
					lb = GetTypeCreateLabel(_type);
				}
				context.AddItem(path + lb, CreateAsset);
			}
			else
			{
				var groupLabel = string.IsNullOrEmpty(label)
				? GetTypeCreateLabel(_type)
				: label;

				foreach (var subType in TypeCache.GetTypesDerivedFrom(_type))
				{
					if (!CanCreateType(subType))
					{
						continue;
					}
					context.AddItem(path + groupLabel + "/" + GetTypeCreateLabel(subType), () =>
					{
						UnityUtility.StartCreatingAsset(subType);
					});
				}
			}
		}

		private static bool CanCreateType(Type type)
		{
			return !type.IsAbstract
			&& typeof(UnityEngine.Object).IsAssignableFrom(type)
			&& (type.GetConstructor(Type.EmptyTypes) != null)
			&& !type.IsDefined(typeof(ObsoleteAttribute));
		}

		private Type GetMonoType(string guid)
		{
			var ms = AssetDatabase.LoadAssetAtPath<MonoScript>(AssetDatabase.GUIDToAssetPath(guid));
			return ms?.GetClass();
		}

		private static string GetTypeCreateLabel(Type type)
		{
			if (type.IsAbstract)
			{
				return ObjectNames.NicifyVariableName(type.Name);
			}
			var attr = type.GetCustomAttribute<CreateAssetMenuAttribute>();
			var menuPath = attr?.menuName ?? string.Empty;

			if (menuPath.Length > 0)
			{
				return menuPath.Substring(menuPath.LastIndexOf('/') + 1);
			}
			return ObjectNames.NicifyVariableName(type.Name);
		}

		private void CreateAsset()
		{
			UnityUtility.StartCreatingAsset(_type);
		}
	}
}