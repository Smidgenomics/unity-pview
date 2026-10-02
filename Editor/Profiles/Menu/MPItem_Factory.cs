// smidgens @ github

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using System;
	using System.Reflection;
	using Newtonsoft.Json;

	using FactoryOutput = System.Collections.Generic.IEnumerable<System.ValueTuple<string,System.Action>>;
	
	// generic item, returns list of options
	[TypeAlias("factory")]
	internal sealed class MPItem_Factory : PVMenuProfileItem
	{
		[JsonProperty("fn")] private string _methodRef;

		private Func<FactoryOutput> _fn;

		protected override void OnDeserialized()
		{
			var m = ParseMethodRef(_methodRef, typeof(FactoryOutput), Type.EmptyTypes);
			_fn = m != null
			? (Func<FactoryOutput>)m.CreateDelegate(typeof(Func<FactoryOutput>))
			: null;
		}

		private static MethodInfo ParseMethodRef(string mRef, Type rType, Type[] pTypes)
		{
			mRef ??= string.Empty;
			int sepIndex = mRef.IndexOf(';');
			if (sepIndex < 0)
			{
				return null;
			}

			var mName = mRef[..sepIndex].Trim();
			var tName = mRef[(sepIndex + 1)..].Trim();

			var type = Type.GetType(tName);
			if (type == null)
			{
				return null;
			}
			var bf = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
			var m = type.GetMethod(mName, bf, null, pTypes, null);
			if (m == null || m.ReturnType != rType)
			{
				return null;
			}
			return m;
		}

		public override void PopulateMenu(string path, MenuGenContext ctx)
		{
			if (_fn == null)
			{
				return;
			}

			if (!string.IsNullOrEmpty(path))
			{
				path += "/";
			}

			foreach (var o in _fn.Invoke())
			{
				ctx.AddItem(path + o.Item1, o.Item2);
			}
			
		}
	}
}