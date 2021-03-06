﻿using ReactiveUI;
using System.Windows;
using System.Windows.Controls;
using System.Reactive.Disposables;
using MagnumOpus.Dialog;

namespace MagnumOpus.IPAddress
{
    /// <summary>
    /// Interaction logic for IPPanel.xaml
    /// </summary>
    public partial class IPAddressPanel : UserControl, IViewFor<IPAddressPanelViewModel>
    {
        public IPAddressPanel()
        {
            InitializeComponent();

            ViewModel = new IPAddressPanelViewModel();

            this.WhenActivated(d =>
            {
                this.Bind(ViewModel, vm => vm.IPAddress, v => v.IPAddress).DisposeWith(d);

                this.OneWayBind(ViewModel, vm => vm.IPAddress, v => v.IPAddressTextBlock.Text, ipAddress => $"IP {ipAddress}").DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.ComputerName, v => v.HostNameTextBlock.Text, hostName => $"({hostName})").DisposeWith(d);
            });
        }

        public Interaction<MessageInfo, int> Messages => ViewModel.Messages;

        public string IPAddress { get => (string)GetValue(IPAddressProperty); set => SetValue(IPAddressProperty, value); }
        public static readonly DependencyProperty IPAddressProperty = DependencyProperty.Register(nameof(IPAddress), typeof(string), typeof(IPAddressPanel), new PropertyMetadata(null));

        public IPAddressPanelViewModel ViewModel { get => (IPAddressPanelViewModel)GetValue(ViewModelProperty); set => SetValue(ViewModelProperty, value); }
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(IPAddressPanelViewModel), typeof(IPAddressPanel), new PropertyMetadata(null));

        object IViewFor.ViewModel { get => ViewModel; set => ViewModel = value as IPAddressPanelViewModel; }
    }
}
