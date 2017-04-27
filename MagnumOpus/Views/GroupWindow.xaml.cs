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
                d(this.OneWayBind(ViewModel, vm => vm.Group.CN, v => v.Title, x => x ?? ""));
                d(this.OneWayBind(ViewModel, vm => vm.Group, v => v.GroupDetails.Group));
                d(this.OneWayBind(ViewModel, vm => vm.Group, v => v.GroupDescription.Group));
                d(this.OneWayBind(ViewModel, vm => vm.Group, v => v.GroupGroups.Group));
                d(this.OneWayBind(ViewModel, vm => vm.Group, v => v.GroupNotes.Group));

                d(this.BindCommand(ViewModel, vm => vm.SetGroup, v => v.RefreshHyperlink, ViewModel.WhenAnyValue(x => x.Group.CN)));
                d(this.Events().KeyDown
                    .Where(x => x.Key == System.Windows.Input.Key.F5)
                    .Select(_ => ViewModel.Group.CN)
                    .InvokeCommand(ViewModel.SetGroup));
                d(new List<Interaction<MessageInfo, int>>
                {
                    GroupDetails.Messages,
                    GroupDescription.Messages,
                    GroupGroups.Messages,
                    GroupNotes.Messages
                }.RegisterMessageHandler(ContainerGrid));
                d(GroupGroups
                    .DialogRequests
                    .RegisterDialogHandler(ContainerGrid));
            });
        }
    }
}
