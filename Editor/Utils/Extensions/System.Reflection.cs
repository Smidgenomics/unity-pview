// smidgens @ github

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using System;
	using System.Collections.Generic;
	using System.Reflection;
	using UnityEngine;

	internal static class Reflection_
	{
		public static Type GetInnermostType(this Type t)
		{
			while (t is { IsArray: true })
			{
				t = t.GetElementType();
			}
			return t;
		}
		
		// Find all fields that Unity would default render in the inspector
		public static IReadOnlyList<FieldInfo> FindInspectorFields<T>(this Type owner)
		{
			// NOTE: doesn't always work properly for unity components, flags might need to be different

			var baseType = typeof(T);

			List<FieldInfo> fields = new List<FieldInfo>();
			LinkedList<Type> hierarchy = new LinkedList<Type>(); // linked for efficient prepend

			// traverse parent hierarchy, stop at base type
			Type currentType = owner;
			while (currentType != baseType && currentType != null)
			{
				hierarchy.AddFirst(currentType);
				currentType = currentType.BaseType;
			}

			var fieldFlags = BindingFlags.NonPublic|BindingFlags.Public|BindingFlags.DeclaredOnly|BindingFlags.Instance;

			// append fields in
			// same order as Unity would normally list them
			foreach (Type htype in hierarchy)
			{
				foreach (FieldInfo field in htype.GetFields(fieldFlags))
				{
					if (!IsUnityInspectorField(field))
					{
						continue;
					}
					fields.Add(field);
				}
			}
			return fields;
		}

		// returns true if given field would be drawn by default unity inspector
		// (serializable and not hidden)
		public static bool IsUnityInspectorField(this FieldInfo f)
		{
			// public but explicitly non-serialized
			if (f.IsPublic && f.IsNotSerialized)
			{
				return false;
			}

			// explicitly hidden
			if (f.IsDefined(typeof(HideInInspector)))
			{
				return false;
			}

			// private, non serialized
			if (!f.IsPublic && !f.IsDefined(typeof(SerializeField)))
			{
				return false;
			}

			var fType = f.FieldType;
			if (fType.IsArray)
			{
				fType = fType.GetElementType()!;
			}

			if(!fType.IsSerializable)
			{
				return false;
			}
			
			// at this point, either the field is public, or private and using SerializeField
			return true;
		}
	}
}