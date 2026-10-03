// smidgens @ github

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using System;
	using System.Collections.Generic;
	using System.Reflection;
	using Newtonsoft.Json;
	using UnityEditor;
	using UnityEngine;

	// create asset(s)
	[TypeAlias("create")]
	internal sealed class MPItem_CreateAsset : PVMenuProfileItem
	{
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

			foreach (var (lb, fn) in GetCreateOptions())
			{
				context.AddItem(path + lb, fn);
			}
		}
		
		protected override void OnDeserialized()
		{
			_type = _classType.IsGUID32()
			? GetMonoType(_classType)
			: Type.GetType(_classType);
		}

		[JsonProperty("classType")] private string _classType;
		[JsonProperty("subClasses")] private bool _subClasses; // if true, will add a create option for every subtype

		private Type _type;

		private IReadOnlyList<(string, Action)> _options;

		private IReadOnlyList<(string, Action)> GetCreateOptions()
		{
			if (_options != null)
			{
				return _options;
			}
			List<(string, Action)> l = new();
			_options = l;
			if (_type == null)
			{
				return _options;
			}

			if (!_subClasses)
			{
				l.Add((GetTypeCreateLabel(_type), () => UnityUtility.StartCreatingAsset(_type)));
				return _options;
			}

			var groupLabel = string.IsNullOrEmpty(label)
			? GetTypeCreateLabel(_type)
			: label;

			foreach (var subType in TypeCache.GetTypesDerivedFrom(_type))
			{
				if (!CanCreateType(subType))
				{
					continue;
				}
				l.Add((groupLabel + "/" + GetTypeCreateLabel(subType), () => UnityUtility.StartCreatingAsset(subType)));
			}
			return _options;
		}
		
		private static bool CanCreateType(Type type)
		{
			return type != null
			&& !type.IsAbstract
			&& typeof(UnityEngine.Object).IsAssignableFrom(type)
			&& (type.GetConstructor(Type.EmptyTypes) != null)
			&& !type.IsDefined(typeof(ObsoleteAttribute));
		}

		private static Type GetMonoType(string guid)
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
	}
}