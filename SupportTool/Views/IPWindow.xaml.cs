using ReactiveUI;
using SupportTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SupportTool.Views
{
	/// <summary>
	/// Interaction logic for IPWindow.xaml
	/// </summary>
	public partial class IPWindow : Window, IViewFor<IPWindowViewModel>
	{
		public IPWindow()
		{
			InitializeComponent();

			this.OneWayBind(ViewModel, vm => vm.IPAddress, v => v.Title, x => x ?? "");

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

		public IPWindowViewModel ViewModel
		{
			get { return (IPWindowViewModel)GetValue(ViewModelProperty); }
			set { SetValue(ViewModelProperty, value); }
		}

		public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(IPWindowViewModel), typeof(IPWindow), new PropertyMetadata(new IPWindowViewModel()));

		object IViewFor.ViewModel
		{
			get { return ViewModel; }
			set { ViewModel = value as IPWindowViewModel; }
		}
	}
}
