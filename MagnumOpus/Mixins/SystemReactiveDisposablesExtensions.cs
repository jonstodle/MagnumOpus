namespace System.Reactive.Disposables
{
	public static class SystemReactiveDisposablesExtensions
	{
		public static void AddTo(this IDisposable source, CompositeDisposable cd) => cd.Add(source);
	}
}
