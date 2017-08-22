using ReactiveUI;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using System.Windows.Controls;
using MagnumOpus.Dialog;

namespace MagnumOpus.Computer
{
    /// <summary>
    /// Interaction logic for ComputerWindow.xaml
    /// </summary>
    public partial class ComputerWindow : DetailsWindow<ComputerWindowViewModel>
    {
        public ComputerWindow()
        {
            InitializeComponent();

            ViewModel = new ComputerWindowViewModel();

            this.WhenActivated(d =>
            {
                this.OneWayBind(ViewModel, vm => vm.Computer.CN, v => v.Title, cn => cn ?? "").DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.Computer, v => v.ComputerDetails.Computer).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.Computer.CN, v => v.RemotePanel.HostName).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.Computer, v => v.ComputerManagement.Computer).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.Computer.CN, v => v.PingPanel.HostName).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.Computer, v => v.ComputerGroups.Computer).DisposeWith(d);

                this.BindCommand(ViewModel, vm => vm.SetComputer, v => v.RefreshHyperlink, ViewModel.WhenAnyValue(vm => vm.Computer.CN)).DisposeWith(d);
                this.Events().KeyDown
                    .Where(args => args.Key == System.Windows.Input.Key.F5)
                    .Select(_ => ViewModel.Computer.CN)
                    .InvokeCommand(ViewModel.SetComputer).DisposeWith(d);
                new List<Interaction<MessageInfo, int>>
                {
                    ComputerDetails.Messages,
                    RemotePanel.Messages,
                    ComputerManagement.Messages,
                    PingPanel.Messages,
                    ComputerGroups.Messages
                }.RegisterMessageHandler(ContainerGrid).DisposeWith(d);
                ComputerGroups
                    .DialogRequests
                    .RegisterDialogHandler(ContainerGrid).DisposeWith(d);
            });
        }
    }
}
