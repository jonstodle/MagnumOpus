using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SupportTool.Views
{
	public class DetailsWindow : Window
	{
		private CompositeDisposable _subscriptions = new CompositeDisposable();

		public DetailsWindow()
		{
			Observable.Interval(TimeSpan.FromHours(2), Scheduler.TaskPool)
				.ObserveOnDispatcher()
				.Subscribe(x => this.Close())
				.AddTo(_subscriptions);

			this.Events().Closed
				.Subscribe(x => _subscriptions.Dispose())
				.AddTo(_subscriptions);
		}
	}
}
