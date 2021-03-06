﻿using ReactiveUI;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using System.Windows.Controls;
using MagnumOpus.Dialog;

namespace MagnumOpus.Group
{
    /// <summary>
    /// Interaction logic for GroupWindow.xaml
    /// </summary>
    public partial class GroupWindow : DetailsWindow<GroupWindowViewModel>
    {
        public GroupWindow()
        {
            InitializeComponent();

            ViewModel = new GroupWindowViewModel();

            this.WhenActivated(d =>
            {
                this.OneWayBind(ViewModel, vm => vm.Group.CN, v => v.Title, cn => cn ?? "").DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.Group, v => v.GroupDetails.Group).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.Group, v => v.GroupDescription.Group).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.Group, v => v.GroupGroups.Group).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.Group, v => v.GroupNotes.Group).DisposeWith(d);

                this.BindCommand(ViewModel, vm => vm.SetGroup, v => v.RefreshHyperlink, ViewModel.WhenAnyValue(vm => vm.Group.CN)).DisposeWith(d);
                this.Events().KeyDown
                    .Where(args => args.Key == System.Windows.Input.Key.F5)
                    .Select(_ => ViewModel.Group.CN)
                    .InvokeCommand(ViewModel.SetGroup).DisposeWith(d);
                new List<Interaction<MessageInfo, int>>
                {
                    GroupDetails.Messages,
                    GroupDescription.Messages,
                    GroupGroups.Messages,
                    GroupNotes.Messages
                }.RegisterMessageHandler(ContainerGrid).DisposeWith(d);
                GroupGroups
                    .DialogRequests
                    .RegisterDialogHandler(ContainerGrid).DisposeWith(d);
            });
        }
    }
}
