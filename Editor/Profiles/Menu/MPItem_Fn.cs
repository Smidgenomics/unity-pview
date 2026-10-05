// smidgens @ github

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using System;
	using System.ComponentModel;
	using System.Reflection;
	using Newtonsoft.Json;
	using UnityEditor;
	using FactoryOutput = System.Collections.Generic.IEnumerable<System.ValueTuple<string,System.Action>>;
	using FactoryFn = System.Func<System.Collections.Generic.IEnumerable<System.ValueTuple<string,System.Action>>>;

	// get options from static function
	[TypeAlias("fn")]
	internal sealed class MPItem_Fn : PVMenuProfileItem
	{
		public override void PopulateMenu(string path, MenuGenContext ctx)
		{
			var fn = GetFactoryFunction();
			if (fn == null)
			{
				return;
			}

			if (!string.IsNullOrEmpty(path))
			{
				path += "/";
			}

			foreach (var o in fn.Invoke())
			{
				ctx.AddItem(path + o.Item1, o.Item2);
			}
		}

		[InjectVariables]
		[JsonProperty("fn")] private string _methodRef;

		// init
		private (bool, FactoryFn) _cachedFn;

		private static string GetMethodDisplayName(MethodInfo m)
		{
			var dn = m.GetCustomAttribute<DisplayNameAttribute>();
			return !string.IsNullOrEmpty(dn?.DisplayName)
			? dn.DisplayName
			: ObjectNames.NicifyVariableName(m.Name);
		}

		private FactoryFn GetFactoryFunction()
		{
			if (_cachedFn.Item1)
			{
				return _cachedFn.Item2;
			}

			FactoryFn factoryFn = null;

			var method = PVParse.ParseStaticMethodRef(_methodRef, null, Type.EmptyTypes);

			if (method == null)
			{
				_cachedFn = (true, null);
				return _cachedFn.Item2;
			}

			// multiple options
			if (method.ReturnType == typeof(FactoryOutput))
			{
				factoryFn = (FactoryFn)method.CreateDelegate(typeof(FactoryFn));
			}
			// single function
			else if (method.ReturnType == typeof(void))
			{
				var lb = !string.IsNullOrEmpty(label) ? label : GetMethodDisplayName(method);
				factoryFn = () =>
				{
					return new (string, Action)[]
					{
						(lb, (Action)method.CreateDelegate(typeof(Action)))
					};
				};
			}

			_cachedFn = (true, factoryFn);
			return _cachedFn.Item2;
		}

	}
}