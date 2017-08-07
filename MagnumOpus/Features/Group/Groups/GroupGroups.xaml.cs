using ReactiveUI;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Reactive.Disposables;
using MagnumOpus.Dialog;

namespace MagnumOpus.Group
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
                this.Bind(ViewModel, vm => vm.Group, v => v.Group).DisposeWith(d);

                this.OneWayBind(ViewModel, vm => vm.IsShowingDirectMemberOf, v => v.DirectMemberOfGrid.Visibility).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.DirectMemberOfGroups, v => v.DirectMemberOfListView.ItemsSource).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.SelectedDirectMemberOfGroup, v => v.DirectMemberOfListView.SelectedItem).DisposeWith(d);

                this.Bind(ViewModel, vm => vm.IsShowingMemberOf, v => v.MemberOfToggleButton.IsChecked).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.IsShowingMemberOf, v => v.MemberOfGrid.Visibility).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.FilterString, v => v.MemberOfFilterTextBox.Text).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.UseFuzzy, v => v.UseFuzzyToggleButton.IsChecked).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.AllMemberOfGroupsView, v => v.MemberOfListView.ItemsSource).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.SelectedAllMemberOfGroup, v => v.MemberOfListView.SelectedItem).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.AllMemberOfGroupsView.Count, v => v.ShowingCountRun.Text).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.AllMemberOfGroups.Count, v => v.TotalCountRun.Text).DisposeWith(d);

                this.Bind(ViewModel, vm => vm.IsShowingMembers, v => v.MembersToggleButton.IsChecked).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.IsShowingMembers, v => v.MembersGrid.Visibility).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.Members, v => v.MembersListView.ItemsSource).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.SelectedMember, v => v.MembersListView.SelectedItem).DisposeWith(d);

                _directMemberOfListViewItemDoubleClicks.ToEventCommandSignal().InvokeCommand(ViewModel.FindDirectMemberOfGroup).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.FindDirectMemberOfGroup, v => v.OpenMemberOfMenuItem).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.OpenEditMemberOf, v => v.EditDirectGroupsButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.SaveDirectGroups, v => v.SaveDirectGroupsButton).DisposeWith(d);
                _memberOfListViewItemDoubleClick.ToEventCommandSignal().InvokeCommand(ViewModel.FindAllMemberOfGroup).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.FindAllMemberOfGroup, v => v.OpenMemberOfAllMenuItem).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.SaveAllGroups, v => v.SaveAllGroupsButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.OpenEditMembers, v => v.MembersButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.SaveMembers, v => v.SaveMembersButton).DisposeWith(d);
                _membersListViewItemDoubleClick.ToEventCommandSignal().InvokeCommand(ViewModel.FindMember).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.FindMember, v => v.OpenMembersMenuItem).DisposeWith(d);

                ViewModel
                    .WhenAnyValue(vm => vm.IsShowingMemberOf)
                    .Where(true)
                    .ToSignal()
                    .ObserveOnDispatcher()
                    .InvokeCommand(ViewModel.GetAllGroups).DisposeWith(d);
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
