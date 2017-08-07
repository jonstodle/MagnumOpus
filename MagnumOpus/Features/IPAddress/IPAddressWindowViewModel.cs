using ReactiveUI;
using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using MagnumOpus.Dialog;
using MagnumOpus.Navigation;

namespace MagnumOpus.IPAddress
{
    public class IPAddressWindowViewModel : ViewModelBase, INavigable
    {
        public IPAddressWindowViewModel()
        {
            SetIPAddress = ReactiveCommand.Create<string, string>(ipAddress => ipAddress);

            _ipAddress = SetIPAddress
                .ToProperty(this, vm => vm.IPAddress);

            this.WhenActivated(disposables =>
            {
                SetIPAddress.ThrownExceptions
                    .SelectMany(ex => _messages.Handle(new MessageInfo(MessageType.Error, ex.Message, "Could not load IP address")))
                    .Subscribe()
                    .DisposeWith(disposables);
            });
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
