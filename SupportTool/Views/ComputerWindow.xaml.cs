using ReactiveUI;
using SupportTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
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
	/// Interaction logic for ComputerWindow.xaml
	/// </summary>
	public partial class ComputerWindow : Window, IViewFor<ComputerWindowViewModel>
	{
		public ComputerWindow()
		{
			InitializeComponent();

			ViewModel = new ComputerWindowViewModel();

			this.OneWayBind(ViewModel, vm => vm.Computer.CN, v => v.Title, x => x ?? "");
			this.OneWayBind(ViewModel, vm => vm.Computer, v => v.ComputerDetails.Computer);
			this.OneWayBind(ViewModel, vm => vm.Computer, v => v.RemotePanel.Computer);
			this.OneWayBind(ViewModel, vm => vm.Computer, v => v.PingPanel.Computer);
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
