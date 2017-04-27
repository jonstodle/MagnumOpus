using MagnumOpus.Services.SettingsServices;
using System;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;

namespace MagnumOpus.Views
{
	public class DetailsWindow<TViewModel> : WindowBase<TViewModel> where TViewModel : class
	{
		public DetailsWindow()
		{
            Observable.Merge(
                _keyDownEvents.ToSignal(),
                this.Events().PreviewMouseDown.ToSignal(),
                Observable.Return(Unit.Default))
                .Select(_ => Observable.Interval(TimeSpan.FromHours(SettingsService.Current.DetailsWindowTimeoutLength), TaskPoolScheduler.Default))
                .Switch()
                .ObserveOnDispatcher()
                .Subscribe(x => this.Close())
                .DisposeWith(_subscriptions);

            if (SettingsService.Current.UseEscapeToCloseDetailsWindows)
            {
                _keyDownEvents
                    .Where(x => x.Key == System.Windows.Input.Key.Escape)
                    .Subscribe(_ => this.Close())
                    .DisposeWith(_subscriptions);
            }

			this.Events().Closed
				.Subscribe(x => _subscriptions.Dispose())
				.DisposeWith(_subscriptions);
		}



        private CompositeDisposable _subscriptions = new CompositeDisposable();
    }
}
