using ReactiveUI;
using MagnumOpus.Models;
using MagnumOpus.ViewModels;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MagnumOpus.Controls
{
    /// <summary>
    /// Interaction logic for GroupGroups.xaml
    /// </summary>
    public partial class GroupGroups : UserControl, IViewFor<GroupGroupsViewModel>
    {
        public GroupGroups()
        {
            InitializeComponent();

            ViewModel = new GroupGroupsViewModel();

            this.Bind(ViewModel, vm => vm.IsShowingDirectMemberOf, v => v.DirectMemberOfToggleButton.IsChecked);
            this.WhenActivated(d =>
            {
                d(this.Bind(ViewModel, vm => vm.Group, v => v.Group));

                d(this.OneWayBind(ViewModel, vm => vm.IsShowingDirectMemberOf, v => v.DirectMemberOfGrid.Visibility));
                d(this.OneWayBind(ViewModel, vm => vm.DirectMemberOfGroups, v => v.DirectMemberOfListView.ItemsSource));
                d(this.Bind(ViewModel, vm => vm.SelectedDirectMemberOfGroup, v => v.DirectMemberOfListView.SelectedItem));

                d(this.Bind(ViewModel, vm => vm.IsShowingMemberOf, v => v.MemberOfToggleButton.IsChecked));
                d(this.OneWayBind(ViewModel, vm => vm.IsShowingMemberOf, v => v.MemberOfGrid.Visibility));
                d(this.Bind(ViewModel, vm => vm.FilterString, v => v.MemberOfFilterTextBox.Text));
                d(this.Bind(ViewModel, vm => vm.UseFuzzy, v => v.UseFuzzyToggleButton.IsChecked));
                d(this.OneWayBind(ViewModel, vm => vm.AllMemberOfGroupsView, v => v.MemberOfListView.ItemsSource));
                d(this.Bind(ViewModel, vm => vm.SelectedAllMemberOfGroup, v => v.MemberOfListView.SelectedItem));
                d(this.OneWayBind(ViewModel, vm => vm.AllMemberOfGroupsView.Count, v => v.ShowingCountRun.Text));
                d(this.OneWayBind(ViewModel, vm => vm.AllMemberOfGroups.Count, v => v.TotalCountRun.Text));

                d(this.Bind(ViewModel, vm => vm.IsShowingMembers, v => v.MembersToggleButton.IsChecked));
                d(this.OneWayBind(ViewModel, vm => vm.IsShowingMembers, v => v.MembersGrid.Visibility));
                d(this.OneWayBind(ViewModel, vm => vm.Members, v => v.MembersListView.ItemsSource));
                d(this.Bind(ViewModel, vm => vm.SelectedMember, v => v.MembersListView.SelectedItem));

                d(_directMemberOfListViewItemDoubleClicks.ToEventCommandSignal().InvokeCommand(ViewModel.FindDirectMemberOfGroup));
                d(this.BindCommand(ViewModel, vm => vm.FindDirectMemberOfGroup, v => v.OpenMemberOfMenuItem));
                d(this.BindCommand(ViewModel, vm => vm.OpenEditMemberOf, v => v.EditDirectGroupsButton));
                d(this.BindCommand(ViewModel, vm => vm.SaveDirectGroups, v => v.SaveDirectGroupsButton));
                d(_memberOfListViewItemDoubleClick.ToEventCommandSignal().InvokeCommand(ViewModel.FindAllMemberOfGroup));
                d(this.BindCommand(ViewModel, vm => vm.FindAllMemberOfGroup, v => v.OpenMemberOfAllMenuItem));
                d(this.BindCommand(ViewModel, vm => vm.SaveAllGroups, v => v.SaveAllGroupsButton));
                d(this.BindCommand(ViewModel, vm => vm.OpenEditMembers, v => v.MembersButton));
                d(this.BindCommand(ViewModel, vm => vm.SaveMembers, v => v.SaveMembersButton));
                d(_membersListViewItemDoubleClick.ToEventCommandSignal().InvokeCommand(ViewModel.FindMember));
                d(this.BindCommand(ViewModel, vm => vm.FindMember, v => v.OpenMembersMenuItem));

                d(ViewModel
                .WhenAnyValue(x => x.IsShowingMemberOf)
                .Where(x => x)
                .Select(_ => Unit.Default)
                .ObserveOnDispatcher()
                .InvokeCommand(ViewModel, x => x.GetAllGroups));
            });
        }

        public Interaction<MessageInfo, int> Messages => ViewModel.Messages;

        public Interaction<DialogInfo, Unit> DialogRequests => ViewModel.DialogRequests;

        public GroupObject Group { get => (GroupObject)GetValue(GroupProperty); set => SetValue(GroupProperty, value); }
        public static readonly DependencyProperty GroupProperty = DependencyProperty.Register(nameof(Group), typeof(GroupObject), typeof(GroupGroups), new PropertyMetadata(null));

        public GroupGroupsViewModel ViewModel { get => (GroupGroupsViewModel)GetValue(ViewModelProperty); set => SetValue(ViewModelProperty, value); }
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(GroupGroupsViewModel), typeof(GroupGroups), new PropertyMetadata(null));

        object IViewFor.ViewModel { get => ViewModel; set => ViewModel = value as GroupGroupsViewModel; }

        private Subject<MouseButtonEventArgs> _directMemberOfListViewItemDoubleClicks = new Subject<MouseButtonEventArgs>();
        private void DirectMemberOfListViewItem_DoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e) => _directMemberOfListViewItemDoubleClicks.OnNext(e);

        private Subject<MouseButtonEventArgs> _memberOfListViewItemDoubleClick = new Subject<MouseButtonEventArgs>();
        private void MemberOfListViewItem_DoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e) => _memberOfListViewItemDoubleClick.OnNext(e);

        private Subject<MouseButtonEventArgs> _membersListViewItemDoubleClick = new Subject<MouseButtonEventArgs>();
        private void MembersListViewItem_DoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e) => _membersListViewItemDoubleClick.OnNext(e);
    }
}
