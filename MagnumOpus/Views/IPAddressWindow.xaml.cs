using ReactiveUI;
using MagnumOpus.Models;
using MagnumOpus.ViewModels;
using System.Collections.Generic;
using System.Reactive;
using System.Windows;

namespace MagnumOpus.Views
{
    /// <summary>
    /// Interaction logic for IPAddressWindow.xaml
    /// </summary>
    public partial class IPAddressWindow : DetailsWindow, IViewFor<IPAddressWindowViewModel>
    {
        public IPAddressWindow()
        {
            InitializeComponent();

            ViewModel = new IPAddressWindowViewModel();

            this.WhenActivated(d =>
            {
                d(this.OneWayBind(ViewModel, vm => vm.IPAddress, v => v.Title, x => x ?? ""));
                d(this.OneWayBind(ViewModel, vm => vm.IPAddress, v => v.IPAddressPanel.IPAddress));
                d(this.OneWayBind(ViewModel, vm => vm.IPAddress, v => v.PingPanel.HostName));

                d(new List<Interaction<MessageInfo, int>>
                {
                    IPAddressPanel.Messages,
                    PingPanel.Messages
                }.RegisterMessageHandler(ContainerGrid));
            });
        }

        public IPAddressWindowViewModel ViewModel { get => (IPAddressWindowViewModel)GetValue(ViewModelProperty); set => SetValue(ViewModelProperty, value); }
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(IPAddressWindowViewModel), typeof(IPAddressWindow), new PropertyMetadata(null));

        object IViewFor.ViewModel { get => ViewModel; set => ViewModel = value as IPAddressWindowViewModel; }
    }
}
