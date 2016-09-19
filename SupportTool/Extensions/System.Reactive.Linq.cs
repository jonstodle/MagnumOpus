using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Reactive.Linq
{
	public static class SystemReactiveLinqExtensions
	{
		public static IObservable<T> WhereNotNull<T>(this IObservable<T> source) where T : class => source.Where(x => x != null);

		public static IObservable<bool> IsTrue<T>(this IObservable<bool> source) where T : class => source.Select(x => x == true);

		public static IObservable<bool> IsFalse<T>(this IObservable<bool> source) where T : class => source.Select(x => x == false);

		public static IObservable<Unit> ToSignal<T>(this IObservable<T> source) where T : class => source.Select(_ => Unit.Default);
	}
}
