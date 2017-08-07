using ReactiveUI;
using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Input;
using MagnumOpus.Navigation;

namespace MagnumOpus
{
    public class WindowBase<TViewModel> : Window, IViewFor<TViewModel> where TViewModel : class
    {
        public WindowBase()
        {
            _keyDownEvents = this.GetEvents<KeyEventArgs>(nameof(KeyDown)).Select(ep => ep.EventArgs).Publish().RefCount();
            _keyUpEvents = this.GetEvents<KeyEventArgs>(nameof(KeyUp)).Select(ep => ep.EventArgs).Publish().RefCount();

            this.WhenActivated(d =>
            {
                _keyDownEvents
                    .Where(args => args.Key == Key.F3)
                    .Subscribe(_ => NavigationService.ShowMainWindow())
                    .DisposeWith(d);
            });
        }



        public TViewModel ViewModel { get => (TViewModel)GetValue(ViewModelProperty); set => SetValue(ViewModelProperty, value); }
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(TViewModel), typeof(WindowBase<TViewModel>), new PropertyMetadata(null));
        object IViewFor.ViewModel { get => ViewModel; set => ViewModel = value as TViewModel; }



        protected IObservable<KeyEventArgs> _keyDownEvents;
        protected IObservable<KeyEventArgs> _keyUpEvents;
    }
}
