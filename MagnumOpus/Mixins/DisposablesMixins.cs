namespace System.Reactive.Disposables
{
	public static class DisposablesMixins
	{
		public static void AddTo(this IDisposable source, CompositeDisposable cd) => cd.Add(source);
	}
}
