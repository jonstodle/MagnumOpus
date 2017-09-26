using System;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Input;
using MagnumOpus.Settings;
using Splat;

namespace MagnumOpus
{
	public class DetailsWindow<TViewModel> : WindowBase<TViewModel> where TViewModel : class
	{
		public DetailsWindow()
		{
            Observable.Merge(
                    _keyDownEvents.ToSignal(),
                    this.Events().PreviewMouseDown.ToSignal(),
                    Observable.Return(Unit.Default))
                        .Select(_ => Observable.Interval(TimeSpan.FromHours(_settings.DetailsWindowTimeoutLength), TaskPoolScheduler.Default))
                .Switch()
                .ObserveOnDispatcher()
                .Subscribe(_ => Close())
                .DisposeWith(_subscriptions);

            if (_settings.UseEscapeToCloseDetailsWindows)
            {
                _keyDownEvents
                    .Where(args => args.Key == Key.Escape)
                    .Subscribe(_ => Close())
                    .DisposeWith(_subscriptions);
            }

			this.Events().Closed
				.Subscribe(_ => _subscriptions.Dispose())
				.DisposeWith(_subscriptions);
		}



		private readonly SettingsFacade _settings = Locator.Current.GetService<SettingsFacade>();
        private CompositeDisposable _subscriptions = new CompositeDisposable();
    }
}
