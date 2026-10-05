// smidgens @ github

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using System;
	using System.Collections.Generic;
	using System.Reflection;
	using JetBrains.Annotations;
	using Newtonsoft.Json.Serialization;
	using UnityEditor;
	using UnityEngine;

	/// <summary>
	/// Serialization binder that maps aliases for concrete types with same base
	/// </summary>
	internal sealed class TypeAliasSerializationBinder : DefaultSerializationBinder
	{
		public TypeAliasSerializationBinder(Type baseType)
		{
			foreach (var nType in TypeCache.GetTypesDerivedFrom(baseType))
			{
				if (nType.IsAbstract)
				{
					continue;
				}
				foreach (var attr in nType.GetCustomAttributes<TypeAliasAttribute>())
				{
					AddAlias(attr.alias, nType);
				}
			}
		}

		// type -> aliases
		private readonly Dictionary<Type, List<string>> _typeToAlias = new ();
		private readonly Dictionary<string, Type> _aliasToType = new ();

		private void AddAlias(string alias, Type type)
		{
			if (_aliasToType.TryGetValue(alias, out var value))
			{
				var at = value.Name;
				throw new ArgumentException($"Alias '{alias}' is already in use for type {at}");
			}

			if (!_typeToAlias.TryGetValue(type, out var aList))
			{
				aList = new List<string>();
				_typeToAlias.Add(type, aList);
			}
			aList.Add(alias);
			_aliasToType.Add(alias, type);
		}

		public override void BindToName(Type serializedType, out string assemblyName, out string typeName)
		{
			var aList = _typeToAlias.GetValueOrDefault(serializedType);
			if (aList != null)
			{
				assemblyName = null;
				typeName = aList[0];
			}
			else
			{
				base.BindToName(serializedType, out assemblyName, out typeName);
			}
		}

		public override Type BindToType([CanBeNull] string assemblyName, [CanBeNull] string typeName)
		{
			if (string.IsNullOrEmpty(typeName))
			{
				return null;
			}
			var type = _aliasToType.GetValueOrDefault(typeName);
			if (type != null)
			{
				return type;
			}
			// alias not found, use whatever the default is
			return base.BindToType(assemblyName, typeName);
		}
	}
}
