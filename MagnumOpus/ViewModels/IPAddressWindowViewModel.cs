using ReactiveUI;
using MagnumOpus.Models;
using MagnumOpus.Services.NavigationServices;
using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace MagnumOpus.ViewModels
{
    public class IPAddressWindowViewModel : ViewModelBase, INavigable
    {
        public IPAddressWindowViewModel()
        {
            SetIPAddress = ReactiveCommand.Create<string, string>(ipAddress => ipAddress);

            _ipAddress = SetIPAddress
                .ToProperty(this, x => x.IPAddress);

            (this).WhenActivated((Action<CompositeDisposable>)(disposables =>
            {
                Observable.SelectMany<Exception, int>(this.SetIPAddress
                .ThrownExceptions
, (Func<Exception, IObservable<int>>)(ex => (IObservable<int>)_messages.Handle((MessageInfo)new MessageInfo((MessageType)MessageType.Error, (string)ex.Message, (string)"Could not load IP address"))))
                .Subscribe()
                .DisposeWith(disposables);
            }));
        }



        public ReactiveCommand<string, string> SetIPAddress { get; private set; }
        public string IPAddress => _ipAddress.Value;



        public Task OnNavigatedTo(object parameter)
        {
            if (parameter is string s)
            {
                Observable.Return(s)
                    .InvokeCommand(SetIPAddress);
            }

            return Task.FromResult<object>(null);
        }

        public Task OnNavigatingFrom() => Task.FromResult<object>(null);



        private readonly ObservableAsPropertyHelper<string> _ipAddress;
    }
}
