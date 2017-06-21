using MagnumOpus.Services.NavigationServices;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MagnumOpus.Views
{
    public class WindowBase<TViewModel> : Window, IViewFor<TViewModel> where TViewModel : class
    {
        public WindowBase()
        {
            _keyDownEvents = this.Events<KeyEventArgs>(nameof(KeyDown)).Select(ep => ep.EventArgs).Publish().RefCount();
            _keyUpEvents = this.Events<KeyEventArgs>(nameof(KeyUp)).Select(ep => ep.EventArgs).Publish().RefCount();

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
