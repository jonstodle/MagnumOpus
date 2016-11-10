using ReactiveUI;
using SupportTool.Models;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SupportTool.Controls
{
	/// <summary>
	/// Interaction logic for ComputerManagement.xaml
	/// </summary>
	public partial class ComputerManagement : UserControl, IViewFor<ComputerManagementViewModel>
	{
		public ComputerManagement()
		{
			InitializeComponent();

			ViewModel = new ComputerManagementViewModel();

			this.WhenActivated(d =>
			{
				d(this.BindCommand(ViewModel, vm => vm.RebootComputer, v => v.RebootButton));
				d(this.BindCommand(ViewModel, vm => vm.RunPSExec, v => v.RunPSExecButton));
				d(this.BindCommand(ViewModel, vm => vm.OpenCDrive, v => v.OpenCButton));
				d(this.BindCommand(ViewModel, vm => vm.OpenSccm, v => v.OpenSccmButton));
			});
		}

		public ComputerObject Computer
		{
			get { return (ComputerObject)GetValue(ComputerProperty); }
			set { SetValue(ComputerProperty, value); }
		}

		public static readonly DependencyProperty ComputerProperty = DependencyProperty.Register(nameof(Computer), typeof(ComputerObject), typeof(ComputerManagement), new PropertyMetadata(null, (d,e) => (d as ComputerManagement).ViewModel.Computer = e.NewValue as ComputerObject));

		public ComputerManagementViewModel ViewModel
		{
			get { return (ComputerManagementViewModel)GetValue(ViewModelProperty); }
			set { SetValue(ViewModelProperty, value); }
		}

		public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(ComputerManagementViewModel), typeof(ComputerManagement), new PropertyMetadata(null));

		object IViewFor.ViewModel
		{
			get { return ViewModel; }
			set { ViewModel = value as ComputerManagementViewModel; }
		}
	}
}
