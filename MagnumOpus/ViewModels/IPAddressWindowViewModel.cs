﻿using ReactiveUI;
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
            _setIPAddress = ReactiveCommand.Create<string, string>(ipAddress => ipAddress);

            _ipAddress = _setIPAddress
                .ToProperty(this, x => x.IPAddress);

            this.WhenActivated(disposables =>
            {
                _setIPAddress
                .ThrownExceptions
                .SelectMany(ex => _messages.Handle(new MessageInfo(MessageType.Error, ex.Message, "Could not load IP address")))
                .Subscribe()
                .DisposeWith(disposables);
            });
        }



        public ReactiveCommand SetIPAddress => _setIPAddress;

        public string IPAddress => _ipAddress.Value;



        public Task OnNavigatedTo(object parameter)
        {
            if (parameter is string s)
            {
                Observable.Return(s)
                    .InvokeCommand(_setIPAddress);
            }

            return Task.FromResult<object>(null);
        }

        public Task OnNavigatingFrom() => Task.FromResult<object>(null);



        private readonly ReactiveCommand<string, string> _setIPAddress;
        private readonly ObservableAsPropertyHelper<string> _ipAddress;
    }
}
