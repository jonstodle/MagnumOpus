using SupportTool.Services.SettingsServices;
using System;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;

namespace SupportTool.Views
{
	public class DetailsWindow : Window
	{
		private CompositeDisposable _subscriptions = new CompositeDisposable();

		public DetailsWindow()
		{
			Observable.Interval(TimeSpan.FromHours(SettingsService.Current.DetailsWindowTimeoutLength), TaskPoolScheduler.Default)
				.ObserveOnDispatcher()
				.Subscribe(x => this.Close())
				.DisposeWith(_subscriptions);

            if (SettingsService.Current.UseEscapeToCloseDetailsWindows)
            {
                this.Events().KeyDown
                    .Where(x => x.Key == System.Windows.Input.Key.Escape)
                    .Subscribe(_ => this.Close())
                    .DisposeWith(_subscriptions);
            }

			this.Events().Closed
				.Subscribe(x => _subscriptions.Dispose())
				.DisposeWith(_subscriptions);
		}
	}
}
