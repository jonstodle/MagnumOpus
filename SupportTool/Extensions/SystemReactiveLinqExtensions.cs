using System.Reactive.Concurrency;
using System.Reactive.Subjects;

namespace System.Reactive.Linq
{
	public static class SystemReactiveLinqExtensions
	{
		public static IObservable<T> WhereNotNull<T>(this IObservable<T> source) where T : class => source.Where(x => x != null);

		public static IObservable<bool> IsNotNull<T>(this IObservable<T> source) where T : class => source.Select(x => x != null);

		public static IObservable<bool> IsTrue<T>(this IObservable<bool> source) where T : class => source.Select(x => x == true);

		public static IObservable<bool> IsFalse<T>(this IObservable<bool> source) where T : class => source.Select(x => x == false);

		public static IObservable<Unit> ToSignal<T>(this IObservable<T> source) where T : class => source.Select(_ => Unit.Default);

        public static IObservable<Unit> ToEventCommandSignal<T>(this IObservable<T> source) where T : class => source.Select(_ => Unit.Default).Delay(TimeSpan.FromMilliseconds(10), DispatcherScheduler.Current);

        public static void AddTo<T>(this IObservable<T> source, MergeObject<T> mergeObject) => mergeObject.Add(source);
	}

	public class MergeObject<T> : IDisposable
	{
		public MergeObject()
		{
			var signals = _signalSubject.SelectMany(x => x).Publish();
			signals.Connect();
			Signals = signals;
		}

		public IObservable<T> Signals { get; private set; }

		public void Add(IObservable<T> observable) => _signalSubject.OnNext(observable);

		public void Dispose() => _signalsSubscription?.Dispose();

		private ReplaySubject<IObservable<T>> _signalSubject;
		private IDisposable _signalsSubscription;

		~MergeObject() => Dispose();
	}
}
