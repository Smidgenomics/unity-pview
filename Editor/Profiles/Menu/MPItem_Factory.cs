// smidgens @ github

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using System;
	using Newtonsoft.Json;
	using FactoryOutput = System.Collections.Generic.IEnumerable<System.ValueTuple<string,System.Action>>;

	// generic item, returns list of options
	[TypeAlias("factory")]
	internal sealed class MPItem_Factory : PVMenuProfileItem
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

		[JsonProperty("fn")] private string _methodRef;

		// init
		private (bool, Func<FactoryOutput>) _cachedFn;
		
		private Func<FactoryOutput> GetFactoryFunction()
		{
			if (_cachedFn.Item1)
			{
				return _cachedFn.Item2;
			}
			var m = PVParse.ParseStaticMethodRef(_methodRef, typeof(FactoryOutput), Type.EmptyTypes);
			var fn = m != null
			? (Func<FactoryOutput>)m.CreateDelegate(typeof(Func<FactoryOutput>))
			: null;
			_cachedFn = (true, fn);
			return _cachedFn.Item2;
		}

	}
}