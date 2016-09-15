using ReactiveUI;
using SupportTool.ViewModels;
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

			this.OneWayBind(ViewModel, vm => vm.IPAddress, v => v.IPAddressTextBlock.Text, x => $"IP {x}");

			this.WhenActivated(d =>
			{
				d(this.BindCommand(ViewModel, vm => vm.OpenLoggedOn, v => v.LoggedOnButton));
				d(this.BindCommand(ViewModel, vm => vm.OpenLoggedOnPlus, v => v.LoggedOnPlusButton));
				d(this.BindCommand(ViewModel, vm => vm.OpenRemoteExecution, v => v.RemoteExecutionButton));
				d(this.BindCommand(ViewModel, vm => vm.OpenCDrive, v => v.OpenCButton));
				d(this.BindCommand(ViewModel, vm => vm.RebootComputer, v => v.RebootButton));
				d(this.BindCommand(ViewModel, vm => vm.StartRemoteControl, v => v.RemoteControlButton));
				d(this.BindCommand(ViewModel, vm => vm.StartRdp, v => v.RdpButton));
			});
		}

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

		public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(IPAddressPanelViewModel), typeof(IPAddressPanel), new PropertyMetadata(new IPAddressPanelViewModel()));

		object IViewFor.ViewModel
		{
			get { return ViewModel; }
			set { ViewModel = value as IPAddressPanelViewModel; }
		}
	}
}
