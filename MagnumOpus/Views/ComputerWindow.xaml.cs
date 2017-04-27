using ReactiveUI;
using MagnumOpus.Models;
using MagnumOpus.ViewModels;
using System.Collections.Generic;
using System.Reactive;
using System.Windows;
using System.Reactive.Linq;

namespace MagnumOpus.Views
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
                d(this.OneWayBind(ViewModel, vm => vm.Computer.CN, v => v.Title, x => x ?? ""));
                d(this.OneWayBind(ViewModel, vm => vm.Computer, v => v.ComputerDetails.Computer));
                d(this.OneWayBind(ViewModel, vm => vm.Computer, v => v.RemotePanel.Computer));
                d(this.OneWayBind(ViewModel, vm => vm.Computer, v => v.ComputerManagement.Computer));
                d(this.OneWayBind(ViewModel, vm => vm.Computer.CN, v => v.PingPanel.HostName));
                d(this.OneWayBind(ViewModel, vm => vm.Computer, v => v.ComputerGroups.Computer));

                d(this.BindCommand(ViewModel, vm => vm.SetComputer, v => v.RefreshHyperlink, ViewModel.WhenAnyValue(x => x.Computer.CN)));
                d(this.Events().KeyDown
                    .Where(x => x.Key == System.Windows.Input.Key.F5)
                    .Select(_ => ViewModel.Computer.CN)
                    .InvokeCommand(ViewModel.SetComputer));
                d(new List<Interaction<MessageInfo, int>>
                {
                    ComputerDetails.Messages,
                    RemotePanel.Messages,
                    ComputerManagement.Messages,
                    PingPanel.Messages,
                    ComputerGroups.Messages
                }.RegisterMessageHandler(ContainerGrid));
                d(ComputerGroups
                    .DialogRequests
                    .RegisterDialogHandler(ContainerGrid));
            });
        }
    }
}
