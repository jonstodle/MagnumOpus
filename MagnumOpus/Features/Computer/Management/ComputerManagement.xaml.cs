using ReactiveUI;
using System.Windows;
using System.Windows.Controls;
using System.Reactive.Disposables;
using MagnumOpus.Dialog;

namespace MagnumOpus.Computer
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
                this.Bind(ViewModel, vm => vm.Computer, v => v.Computer).DisposeWith(d);

                this.BindCommand(ViewModel, vm => vm.RebootComputer, v => v.RebootButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.RunPSExec, v => v.RunPSExecButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.OpenCDrive, v => v.OpenCButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.OpenSccm, v => v.OpenSccmButton).DisposeWith(d);
            });
        }

        public Interaction<MessageInfo, int> Messages => ViewModel.Messages;

        public ComputerObject Computer { get => (ComputerObject)GetValue(ComputerProperty); set => SetValue(ComputerProperty, value); }
        public static readonly DependencyProperty ComputerProperty = DependencyProperty.Register(nameof(Computer), typeof(ComputerObject), typeof(ComputerManagement), new PropertyMetadata(null));

        public ComputerManagementViewModel ViewModel { get => (ComputerManagementViewModel)GetValue(ViewModelProperty); set => SetValue(ViewModelProperty, value); }
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(ComputerManagementViewModel), typeof(ComputerManagement), new PropertyMetadata(null));

        object IViewFor.ViewModel { get => ViewModel; set => ViewModel = value as ComputerManagementViewModel; }
    }
}
