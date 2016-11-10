using ReactiveUI;
using SupportTool.ViewModels;
using System.Windows;

namespace SupportTool.Views
{
	/// <summary>
	/// Interaction logic for ComputerWindow.xaml
	/// </summary>
	public partial class ComputerWindow : DetailsWindow, IViewFor<ComputerWindowViewModel>
	{
		public ComputerWindow()
		{
			InitializeComponent();

			ViewModel = new ComputerWindowViewModel();

			this.OneWayBind(ViewModel, vm => vm.Computer.CN, v => v.Title, x => x ?? "");
			this.OneWayBind(ViewModel, vm => vm.Computer, v => v.ComputerDetails.Computer);
			this.OneWayBind(ViewModel, vm => vm.Computer, v => v.RemotePanel.Computer);
			this.OneWayBind(ViewModel, vm => vm.Computer, v => v.ComputerManagement.Computer);
			this.OneWayBind(ViewModel, vm => vm.Computer.CN, v => v.PingPanel.HostName);
			this.OneWayBind(ViewModel, vm => vm.Computer, v => v.ComputerGroups.Computer);

			this.WhenActivated(d =>
			{
				d(this.BindCommand(ViewModel, vm => vm.SetComputer, v => v.RefreshHyperlink, ViewModel.WhenAnyValue(x => x.Computer.CN)));
			});
		}

		public ComputerWindowViewModel ViewModel
		{
			get { return (ComputerWindowViewModel)GetValue(ViewModelProperty); }
			set { SetValue(ViewModelProperty, value); }
		}

		public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(ComputerWindowViewModel), typeof(ComputerWindow), new PropertyMetadata(null));

		object IViewFor.ViewModel
		{
			get { return ViewModel; }
			set { ViewModel = value as ComputerWindowViewModel; }
		}
	}
}
