using ReactiveUI;
using SupportTool.Models;
using SupportTool.ViewModels;
using System.Reactive;
using System.Windows;
using System.Windows.Controls;

namespace SupportTool.Controls
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

			this.OneWayBind(ViewModel, vm => vm.IPAddress, v => v.IPAddressTextBlock.Text, x => $"IP {x}");
			this.OneWayBind(ViewModel, vm => vm.HostName, v => v.HostNameTextBlock.Text, x => $"({x})");

			this.WhenActivated(d =>
			{
				d(this.BindCommand(ViewModel, vm => vm.OpenLoggedOn, v => v.LoggedOnButton));
				d(this.BindCommand(ViewModel, vm => vm.OpenLoggedOnPlus, v => v.LoggedOnPlusButton));
				d(this.BindCommand(ViewModel, vm => vm.OpenRemoteExecution, v => v.RemoteExecutionButton));
				d(this.BindCommand(ViewModel, vm => vm.OpenCDrive, v => v.OpenCButton));
				d(this.BindCommand(ViewModel, vm => vm.RebootComputer, v => v.RebootButton));
				d(this.BindCommand(ViewModel, vm => vm.StartRemoteControl, v => v.RemoteControlButton));
				d(this.BindCommand(ViewModel, vm => vm.StartRemoteControl2012, v => v.RemoteControl2012Button));
				d(this.BindCommand(ViewModel, vm => vm.KillRemoteControl, v => v.KillRemoteConrolButton));
				d(this.BindCommand(ViewModel, vm => vm.StartRemoteAssistance, v => v.RemoteAssistanceButton));
				d(this.BindCommand(ViewModel, vm => vm.StartRdp, v => v.RdpButton));
			});
		}

		public Interaction<MessageInfo, Unit> InfoMessages => ViewModel.InfoMessages;

		public Interaction<MessageInfo, Unit> ErrorMessages => ViewModel.ErrorMessages;

		public string IPAddress
		{
			get { return (string)GetValue(IPAddressProperty); }
			set { SetValue(IPAddressProperty, value); }
		}

		public static readonly DependencyProperty IPAddressProperty = DependencyProperty.Register(nameof(IPAddress), typeof(string), typeof(IPAddressPanel), new PropertyMetadata(null, (d, e) => (d as IPAddressPanel).ViewModel.IPAddress = e.NewValue as string));

		public IPAddressPanelViewModel ViewModel
		{
			get { return (IPAddressPanelViewModel)GetValue(ViewModelProperty); }
			set { SetValue(ViewModelProperty, value); }
		}

		public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(IPAddressPanelViewModel), typeof(IPAddressPanel), new PropertyMetadata(null));

		object IViewFor.ViewModel
		{
			get { return ViewModel; }
			set { ViewModel = value as IPAddressPanelViewModel; }
		}
	}
}
