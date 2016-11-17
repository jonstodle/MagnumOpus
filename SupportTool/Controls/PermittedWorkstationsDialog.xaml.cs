using ReactiveUI;
using SupportTool.Models;
using SupportTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
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
	/// Interaction logic for PermittedWorkstationsDialog.xaml
	/// </summary>
	public partial class PermittedWorkstationsDialog : UserControl, IViewFor<PermittedWorkstationsDialogViewModel>
	{
		public PermittedWorkstationsDialog()
		{
			InitializeComponent();

			ViewModel = new PermittedWorkstationsDialogViewModel();

			//this.OneWayBind(ViewModel, vm => vm.WindowTitle, v => v.Title, x => x != null ? x : "");

			this.Bind(ViewModel, vm => vm.ComputerName, v => v.ComputerNameTextBox.Text);
			this.OneWayBind(ViewModel, vm => vm.ComputersView, v => v.ComputersListView.ItemsSource);
			this.Bind(ViewModel, vm => vm.SelectedComputer, v => v.ComputersListView.SelectedItem);

			this.WhenActivated(d =>
			{
				ComputerNameTextBox.Focus();

				d(this.BindCommand(ViewModel, vm => vm.AddComputer, v => v.AddComputerButton));
				d(this.BindCommand(ViewModel, vm => vm.RemoveComputer, v => v.RemoveComputerButton));
				d(this.BindCommand(ViewModel, vm => vm.RemoveAllComputers, v => v.RemoveAllComputersButton));
				d(this.BindCommand(ViewModel, vm => vm.Save, v => v.SaveButton));
				d(ComputersListView.Events()
					.MouseDoubleClick
					.Select(_ => Unit.Default)
					.InvokeCommand(ViewModel, x => x.RemoveComputer));
				d(ViewModel
					.InfoMessages
					.RegisterInfoHandler(ContainerGrid));
				d(ViewModel
					.ErrorMessages
					.RegisterErrorHandler(ContainerGrid));
			});
		}

		public PermittedWorkstationsDialogViewModel ViewModel
		{
			get { return (PermittedWorkstationsDialogViewModel)GetValue(ViewModelProperty); }
			set { SetValue(ViewModelProperty, value); }
		}

		public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(PermittedWorkstationsDialogViewModel), typeof(PermittedWorkstationsDialog), new PropertyMetadata(null));

		object IViewFor.ViewModel
		{
			get { return ViewModel; }
			set { ViewModel = value as PermittedWorkstationsDialogViewModel; }
		}
	}
}
