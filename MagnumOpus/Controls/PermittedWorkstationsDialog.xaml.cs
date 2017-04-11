﻿using ReactiveUI;
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

                d(this.OneWayBind(ViewModel, vm => vm.User, v => v.TitleTextBlock.Text, x => x != null ? $"Permitted Workstations for {x.Principal.Name}" : "Permitted Workstations"));
                d(this.Bind(ViewModel, vm => vm.ComputerName, v => v.ComputerNameTextBox.Text));
                d(this.OneWayBind(ViewModel, vm => vm.Computers, v => v.ComputersListView.ItemsSource));
                d(this.Bind(ViewModel, vm => vm.SelectedComputer, v => v.ComputersListView.SelectedItem));

                d(this.BindCommand(ViewModel, vm => vm.AddComputer, v => v.AddComputerButton));
                d(this.BindCommand(ViewModel, vm => vm.RemoveComputer, v => v.RemoveComputerButton));
                d(this.BindCommand(ViewModel, vm => vm.RemoveAllComputers, v => v.RemoveAllComputersButton));
                d(this.BindCommand(ViewModel, vm => vm.Save, v => v.SaveButton));
                d(this.BindCommand(ViewModel, vm => vm.Cancel, v => v.CancelButton));
                d(ComputersListView.Events()
                    .MouseDoubleClick
                    .Select(_ => Unit.Default)
                    .InvokeCommand(ViewModel, x => x.RemoveComputer));
                d(this.BindCommand(ViewModel, vm => vm.RemoveComputer, v => v.RemoveComputerMenuItem));
                d(ViewModel
                    .Messages
                    .RegisterMessageHandler(ContainerGrid));
            });
        }

        public PermittedWorkstationsDialogViewModel ViewModel { get => (PermittedWorkstationsDialogViewModel)GetValue(ViewModelProperty); set => SetValue(ViewModelProperty, value); }
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(PermittedWorkstationsDialogViewModel), typeof(PermittedWorkstationsDialog), new PropertyMetadata(null));

        object IViewFor.ViewModel { get => ViewModel; set => ViewModel = value as PermittedWorkstationsDialogViewModel; }
    }
}
