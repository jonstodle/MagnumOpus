using ReactiveUI;
using MagnumOpus.Models;
using MagnumOpus.ViewModels;
using System.Collections.Generic;
using System.Reactive;
using System.Windows;
using System.Reactive.Disposables;

namespace MagnumOpus.Views
{
    /// <summary>
    /// Interaction logic for IPAddressWindow.xaml
    /// </summary>
    public partial class IPAddressWindow : DetailsWindow<IPAddressWindowViewModel>
    {
        public IPAddressWindow()
        {
            InitializeComponent();

            ViewModel = new IPAddressWindowViewModel();

            this.WhenActivated(d =>
            {
                this.OneWayBind(ViewModel, vm => vm.IPAddress, v => v.Title, x => x ?? "").DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.IPAddress, v => v.IPAddressPanel.IPAddress).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.IPAddress, v => v.PingPanel.HostName).DisposeWith(d);

                new List<Interaction<MessageInfo, int>>
                {
                    IPAddressPanel.Messages,
                    PingPanel.Messages
                }.RegisterMessageHandler(ContainerGrid).DisposeWith(d);
            });
        }
    }
}
