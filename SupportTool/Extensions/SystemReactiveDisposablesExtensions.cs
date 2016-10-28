using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Reactive.Disposables
{
	public static class SystemReactiveDisposablesExtensions
	{
		public static void AddTo(this IDisposable source, CompositeDisposable cd) => cd.Add(source);
	}
}
