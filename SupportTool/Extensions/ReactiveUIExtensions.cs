using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReactiveUI
{
	public static class ReactiveUIExtensions
	{
		public static void HandleErrorsWith(this ReactiveCommand source, MergeObject<Exception> mergeObject) => mergeObject.Add(source.ThrownExceptions);
	}
}
