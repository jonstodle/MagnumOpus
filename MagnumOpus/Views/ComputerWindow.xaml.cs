using ReactiveUI;
using MagnumOpus.Models;
using MagnumOpus.ViewModels;
using System.Collections.Generic;
using System.Reactive;
using System.Windows;

namespace MagnumOpus.Views
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

            this.WhenActivated(d =>
            {
                d(this.OneWayBind(ViewModel, vm => vm.Computer.CN, v => v.Title, x => x ?? ""));
                d(this.OneWayBind(ViewModel, vm => vm.Computer, v => v.ComputerDetails.Computer));
                d(this.OneWayBind(ViewModel, vm => vm.Computer, v => v.RemotePanel.Computer));
                d(this.OneWayBind(ViewModel, vm => vm.Computer, v => v.ComputerManagement.Computer));
                d(this.OneWayBind(ViewModel, vm => vm.Computer.CN, v => v.PingPanel.HostName));
                d(this.OneWayBind(ViewModel, vm => vm.Computer, v => v.ComputerGroups.Computer));

                d(this.BindCommand(ViewModel, vm => vm.SetComputer, v => v.RefreshHyperlink, ViewModel.WhenAnyValue(x => x.Computer.CN)));
                d(ComputerManagement
                    .PromptMessages
                    .RegisterPromptHandler(ContainerGrid));
                d(new List<Interaction<MessageInfo, Unit>>
                {
                    ComputerDetails.InfoMessages,
                    RemotePanel.InfoMessages,
                    ComputerManagement.InfoMessages,
                    PingPanel.InfoMessages,
                    ComputerGroups.InfoMessages
                }.RegisterInfoHandler(ContainerGrid));
                d(new List<Interaction<MessageInfo, Unit>>
                {
                    ComputerDetails.ErrorMessages,
                    RemotePanel.ErrorMessages,
                    ComputerManagement.ErrorMessages,
                    PingPanel.ErrorMessages,
                    ComputerGroups.ErrorMessages
                }.RegisterErrorHandler(ContainerGrid));
                d(ComputerGroups
                    .DialogRequests
                    .RegisterDialogHandler(ContainerGrid));
            });
        }

        public ComputerWindowViewModel ViewModel { get => (ComputerWindowViewModel)GetValue(ViewModelProperty); set => SetValue(ViewModelProperty, value); }
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(ComputerWindowViewModel), typeof(ComputerWindow), new PropertyMetadata(null));

        object IViewFor.ViewModel { get => ViewModel; set => ViewModel = value as ComputerWindowViewModel; }
    }
}
