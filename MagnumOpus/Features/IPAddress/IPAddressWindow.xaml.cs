using ReactiveUI;
using System.Collections.Generic;
using System.Reactive.Disposables;
using MagnumOpus.Dialog;

namespace MagnumOpus.IPAddress
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
                this.OneWayBind(ViewModel, vm => vm.IPAddress, v => v.Title, ipAddress => ipAddress ?? "").DisposeWith(d);
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
