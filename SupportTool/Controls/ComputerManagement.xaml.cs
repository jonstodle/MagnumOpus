using ReactiveUI;
using SupportTool.Models;
using SupportTool.ViewModels;
using System.Reactive;
using System.Windows;
using System.Windows.Controls;

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
                d(this.Bind(ViewModel, vm => vm.Computer, v => v.Computer));

                d(this.BindCommand(ViewModel, vm => vm.RebootComputer, v => v.RebootButton));
                d(this.BindCommand(ViewModel, vm => vm.RunPSExec, v => v.RunPSExecButton));
                d(this.BindCommand(ViewModel, vm => vm.OpenCDrive, v => v.OpenCButton));
                d(this.BindCommand(ViewModel, vm => vm.OpenSccm, v => v.OpenSccmButton));
            });
        }

        public Interaction<MessageInfo, int> PromptMessages => ViewModel.PromptMessages;

        public Interaction<MessageInfo, Unit> InfoMessages => ViewModel.InfoMessages;

        public Interaction<MessageInfo, Unit> ErrorMessages => ViewModel.ErrorMessages;

        public ComputerObject Computer { get => (ComputerObject)GetValue(ComputerProperty); set => SetValue(ComputerProperty, value); }
        public static readonly DependencyProperty ComputerProperty = DependencyProperty.Register(nameof(Computer), typeof(ComputerObject), typeof(ComputerManagement), new PropertyMetadata(null));

        public ComputerManagementViewModel ViewModel { get => (ComputerManagementViewModel)GetValue(ViewModelProperty); set => SetValue(ViewModelProperty, value); }
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(ComputerManagementViewModel), typeof(ComputerManagement), new PropertyMetadata(null));

        object IViewFor.ViewModel { get => ViewModel; set => ViewModel = value as ComputerManagementViewModel; }
    }
}
