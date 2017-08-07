using ReactiveUI;
using MagnumOpus.Models;
using MagnumOpus.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Reactive.Disposables;

namespace MagnumOpus.Controls
{
    /// <summary>
    /// Interaction logic for IPPanel.xaml
    /// </summary>
    public partial class IPAddressPanel : UserControl, IViewFor<IPAddressPanelViewModel>
    {
        public IPAddressPanel()
        {
            InitializeComponent();

            ViewModel = new IPAddressPanelViewModel();

            this.WhenActivated(d =>
            {
                this.Bind(ViewModel, vm => vm.IPAddress, v => v.IPAddress).DisposeWith(d);

                this.OneWayBind(ViewModel, vm => vm.IPAddress, v => v.IPAddressTextBlock.Text, ipAddress => $"IP {ipAddress}").DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.HostName, v => v.HostNameTextBlock.Text, hostName => $"({hostName})").DisposeWith(d);

                this.BindCommand(ViewModel, vm => vm.OpenLoggedOn, v => v.LoggedOnButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.OpenLoggedOnPlus, v => v.LoggedOnPlusButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.OpenRemoteExecution, v => v.RemoteExecutionButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.OpenCDrive, v => v.OpenCButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.RebootComputer, v => v.RebootButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.StartRemoteControl, v => v.RemoteControlButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.StartRemoteControl2012, v => v.RemoteControl2012Button).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.KillRemoteControl, v => v.KillRemoteConrolButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.StartRemoteAssistance, v => v.RemoteAssistanceButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.StartRdp, v => v.RdpButton).DisposeWith(d);
            });
        }

        public Interaction<MessageInfo, int> Messages => ViewModel.Messages;

        public string IPAddress { get => (string)GetValue(IPAddressProperty); set => SetValue(IPAddressProperty, value); }
        public static readonly DependencyProperty IPAddressProperty = DependencyProperty.Register(nameof(IPAddress), typeof(string), typeof(IPAddressPanel), new PropertyMetadata(null));

        public IPAddressPanelViewModel ViewModel { get => (IPAddressPanelViewModel)GetValue(ViewModelProperty); set => SetValue(ViewModelProperty, value); }
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(IPAddressPanelViewModel), typeof(IPAddressPanel), new PropertyMetadata(null));

        object IViewFor.ViewModel { get => ViewModel; set => ViewModel = value as IPAddressPanelViewModel; }
    }
}
