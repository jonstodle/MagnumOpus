using System.Reactive.Concurrency;
using System.Reactive.Subjects;

namespace System.Reactive.Linq
{
	public static class SystemReactiveLinqExtensions
	{
		public static IObservable<T> WhereNotNull<T>(this IObservable<T> source) where T : class => source.Where(x => x != null);

        public static IObservable<bool> Where(this IObservable<bool> source, bool desiredBool) => source.Where(value => value == desiredBool);

		public static IObservable<bool> IsNotNull<T>(this IObservable<T> source) where T : class => source.Select(x => x != null);

		public static IObservable<bool> IsTrue(this IObservable<bool> source) => source.Select(x => x == true);

		public static IObservable<bool> IsFalse(this IObservable<bool> source) => source.Select(x => x == false);

		public static IObservable<Unit> ToSignal<T>(this IObservable<T> source) => source.Select(_ => Unit.Default);

        public static IObservable<Unit> ToEventCommandSignal<T>(this IObservable<T> source) => source.Select(_ => Unit.Default).Delay(TimeSpan.FromMilliseconds(10), DispatcherScheduler.Current);

        public static IObservable<EventPattern<object>> Events(this object source, string eventName) => Observable.FromEventPattern(source, eventName);

        public static IObservable<EventPattern<TEventArgs>> Events<TEventArgs>(this object source, string eventName) => Observable.FromEventPattern<TEventArgs>(source, eventName);

        public static IObservable<T> CatchAndReturn<T>(this IObservable<T> source, T returnValue) => source.Catch(Observable.Return(returnValue));

        public static IObservable<T> Debug<T>(this IObservable<T> source, string identifier = null) => source.Debug(value => value, identifier);

        public static IObservable<T> Debug<T>(this IObservable<T> source, Func<T, object> selector, string identifier = null)
        {
            var prefix = identifier != null ? identifier + " " : "";
            return source.Do(value => System.Diagnostics.Debug.WriteLine($"{prefix}Next: {selector(value)}"), ex => System.Diagnostics.Debug.WriteLine($"{prefix}Error: {ex}"), () => System.Diagnostics.Debug.WriteLine($"{prefix}Completed"));
        }
    }
}
