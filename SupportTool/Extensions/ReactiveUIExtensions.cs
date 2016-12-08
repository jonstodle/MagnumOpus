using System;
using System.Reactive.Linq;

namespace ReactiveUI
{
	public static class ReactiveUIExtensions
	{
		public static void HandleErrorsWith(this ReactiveCommand source, MergeObject<Exception> mergeObject) => mergeObject.Add(source.ThrownExceptions);
	}
}
