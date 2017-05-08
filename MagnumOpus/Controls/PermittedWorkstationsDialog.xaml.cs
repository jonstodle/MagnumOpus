using ReactiveUI;
using MagnumOpus.Models;
using MagnumOpus.ViewModels;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Reactive.Disposables;

namespace MagnumOpus.Controls
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

            this.WhenActivated(d =>
            {
                ComputerNameTextBox.Focus();

                this.OneWayBind(ViewModel, vm => vm.User, v => v.TitleTextBlock.Text, x => x != null ? $"Permitted Workstations for {x.Principal.Name}" : "Permitted Workstations").DisposeWith(d);
                this.Bind(ViewModel, vm => vm.ComputerName, v => v.ComputerNameTextBox.Text).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.Computers, v => v.ComputersListView.ItemsSource).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.SelectedComputer, v => v.ComputersListView.SelectedItem).DisposeWith(d);

                this.BindCommand(ViewModel, vm => vm.AddComputer, v => v.AddComputerButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.RemoveComputer, v => v.RemoveComputerButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.RemoveAllComputers, v => v.RemoveAllComputersButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.Save, v => v.SaveButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.Cancel, v => v.CancelButton).DisposeWith(d);
                ComputersListView.Events()
                    .MouseDoubleClick
                    .Select(_ => Unit.Default)
                    .InvokeCommand(ViewModel, x => x.RemoveComputer).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.RemoveComputer, v => v.RemoveComputerMenuItem).DisposeWith(d);
                ViewModel
                    .Messages
                    .RegisterMessageHandler(ContainerGrid).DisposeWith(d);
            });
        }

        public PermittedWorkstationsDialogViewModel ViewModel { get => (PermittedWorkstationsDialogViewModel)GetValue(ViewModelProperty); set => SetValue(ViewModelProperty, value); }
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(PermittedWorkstationsDialogViewModel), typeof(PermittedWorkstationsDialog), new PropertyMetadata(null));

        object IViewFor.ViewModel { get => ViewModel; set => ViewModel = value as PermittedWorkstationsDialogViewModel; }
    }
}
