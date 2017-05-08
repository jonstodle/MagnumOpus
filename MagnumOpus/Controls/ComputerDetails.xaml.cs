using ReactiveUI;
using MagnumOpus.Models;
using MagnumOpus.ViewModels;
using System;
using System.Reactive;
using System.Windows;
using System.Windows.Controls;
using System.Reactive.Disposables;

namespace MagnumOpus.Controls
{
    /// <summary>
    /// Interaction logic for ComputerDetails.xaml
    /// </summary>
    public partial class ComputerDetails : UserControl, IViewFor<ComputerDetailsViewModel>
    {
        public ComputerDetails()
        {
            InitializeComponent();

            ViewModel = new ComputerDetailsViewModel();

            this.WhenActivated(d =>
            {
                this.Bind(ViewModel, vm => vm.Computer, v => v.Computer).DisposeWith(d);

                this.OneWayBind(ViewModel, vm => vm.Computer.CN, v => v.CNTextBlock.Text).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.Computer.Company, v => v.CompanyTextBlock.Text).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.Computer.OperatingSystem, v => v.OperatingSystemTextBlock.Text).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.Computer.ServicePack, v => v.OperatingSystemCSDTextBlock.Text, x => x.HasValue() ? $" {x}" : "").DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.OperatingSystemInfo.Architecture, v => v.OperatingSystemArchitectureTextBlock.Text, x => x.HasValue() ? $" {x}" : "").DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.IPAddress, v => v.IPAddressTextBlock.Text).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.IsShowingDetails, v => v.DetailsGrid.Visibility).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.Computer.OperatingSystem, v => v.DetailsOperatingSystemTextBlock.Text).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.Computer.ServicePack, v => v.DetailsOperatingSystemCSDTextBlock.Text, x => x.HasValue() ? $" {x}" : "").DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.OperatingSystemInfo.Architecture, v => v.DetailsOperatingSystemArchitectureTextBlock.Text, x => x.HasValue() ? $" {x}" : "").DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.OperatingSystemInfo.LastBootTime, v => v.LastBootTextBlock.Text, x => x != null ? ((DateTime)x).ToString("HH:mm:ss dd.MM.yyyy") : "Could not get last boot").DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.OperatingSystemInfo.InstallDate, v => v.InstallDateTextBlock.Text, x => x != null ? ((DateTime)x).ToString("HH:mm:ss dd.MM.yyyy") : "Could not get last install").DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.Computer.WhenCreated, v => v.ADCreateDateTextBlock.Text, x => x != null ? ((DateTime)x).ToString("HH:mm:ss dd.MM.yyyy") : "").DisposeWith(d);

                this.BindCommand(ViewModel, vm => vm.ToggleIsShowingDetails, v => v.OperatingSystemHyperlink).DisposeWith(d);
            });
        }

        public Interaction<MessageInfo, int> Messages => ViewModel.Messages;

        public ComputerObject Computer { get => (ComputerObject)GetValue(ComputerProperty); set => SetValue(ComputerProperty, value); }
        public static readonly DependencyProperty ComputerProperty = DependencyProperty.Register(nameof(Computer), typeof(ComputerObject), typeof(ComputerDetails), new PropertyMetadata(null));

        public ComputerDetailsViewModel ViewModel { get => (ComputerDetailsViewModel)GetValue(ViewModelProperty); set => SetValue(ViewModelProperty, value); }
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(ComputerDetailsViewModel), typeof(ComputerDetails), new PropertyMetadata(null));

        object IViewFor.ViewModel { get => ViewModel; set => ViewModel = value as ComputerDetailsViewModel; }
    }
}
