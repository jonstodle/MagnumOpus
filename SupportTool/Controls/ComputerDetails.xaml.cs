﻿using ReactiveUI;
using SupportTool.Models;
using SupportTool.ViewModels;
using System;
using System.Reactive;
using System.Windows;
using System.Windows.Controls;

namespace SupportTool.Controls
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



			this.OneWayBind(ViewModel, vm => vm.Computer.CN, v => v.CNTextBlock.Text);
			this.OneWayBind(ViewModel, vm => vm.Computer.Company, v => v.CompanyTextBlock.Text);
			this.OneWayBind(ViewModel, vm => vm.Computer.OperatingSystem, v => v.OperatingSystemTextBlock.Text);
			this.OneWayBind(ViewModel, vm => vm.Computer.ServicePack, v => v.OperatingSystemCSDTextBlock.Text, x => x.HasValue() ? $" {x}" : "");
			this.OneWayBind(ViewModel, vm => vm.OperatingSystemInfo.Architecture, v => v.OperatingSystemArchitectureTextBlock.Text, x => x.HasValue() ? $" {x}" : "");
            this.OneWayBind(ViewModel, vm => vm.IPAddress, v => v.IPAddressTextBlock.Text);
            this.OneWayBind(ViewModel, vm => vm.IsShowingDetails, v => v.DetailsGrid.Visibility);
            this.OneWayBind(ViewModel, vm => vm.Computer.OperatingSystem, v => v.DetailsOperatingSystemTextBlock.Text);
            this.OneWayBind(ViewModel, vm => vm.Computer.ServicePack, v => v.DetailsOperatingSystemCSDTextBlock.Text, x => x.HasValue() ? $" {x}" : "");
            this.OneWayBind(ViewModel, vm => vm.OperatingSystemInfo.Architecture, v => v.DetailsOperatingSystemArchitectureTextBlock.Text, x => x.HasValue() ? $" {x}" : "");
            this.OneWayBind(ViewModel, vm => vm.OperatingSystemInfo.LastBootTime, v => v.LastBootTextBlock.Text, x => x != null ? ((DateTime)x).ToString("HH:mm:ss dd.MM.yyyy") : "Could not get last boot");
			this.OneWayBind(ViewModel, vm => vm.OperatingSystemInfo.InstallDate, v => v.InstallDateTextBlock.Text, x => x != null ? ((DateTime)x).ToString("HH:mm:ss dd.MM.yyyy") : "Could not get last install");
			this.OneWayBind(ViewModel, vm => vm.Computer.WhenCreated, v => v.ADCreateDateTextBlock.Text, x => x != null ? ((DateTime)x).ToString("HH:mm:ss dd.MM.yyyy"): "");

            this.WhenActivated(d =>
            {
                d(this.BindCommand(ViewModel, vm => vm.ToggleIsShowingDetails, v => v.OperatingSystemHyperlink));
            });
		}

		public Interaction<MessageInfo, Unit> InfoMessages => ViewModel.InfoMessages;

		public Interaction<MessageInfo, Unit> ErrorMessages => ViewModel.ErrorMessages;

		public ComputerObject Computer
        {
            get { return (ComputerObject)GetValue(ComputerProperty); }
            set { SetValue(ComputerProperty, value); }
        }

        public static readonly DependencyProperty ComputerProperty = DependencyProperty.Register(nameof(Computer), typeof(ComputerObject), typeof(ComputerDetails), new PropertyMetadata(null, (d,e)=> (d as ComputerDetails).ViewModel.Computer = e.NewValue as ComputerObject));

        public ComputerDetailsViewModel ViewModel
        {
            get { return (ComputerDetailsViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(ComputerDetailsViewModel), typeof(ComputerDetails), new PropertyMetadata(null));

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = value as ComputerDetailsViewModel; }
        }
    }
}
