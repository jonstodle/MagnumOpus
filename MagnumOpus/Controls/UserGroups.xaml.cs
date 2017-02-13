using ReactiveUI;
using MagnumOpus.Models;
using MagnumOpus.ViewModels;
using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MagnumOpus.Controls
{
    /// <summary>
    /// Interaction logic for UserGroups.xaml
    /// </summary>
    public partial class UserGroups : UserControl, IViewFor<UserGroupsViewModel>
    {
        public UserGroups()
        {
            InitializeComponent();

            ViewModel = new UserGroupsViewModel();

            this.WhenActivated(d =>
            {
                d(this.Bind(ViewModel, vm => vm.User, v => v.User));

                d(this.Bind(ViewModel, vm => vm.IsShowingDirectGroups, v => v.DirectGroupsToggleButton.IsChecked));
                d(this.OneWayBind(ViewModel, vm => vm.IsShowingDirectGroups, v => v.DirectGroupsGrid.Visibility));
                d(this.Bind(ViewModel, vm => vm.IsShowingAllGroups, v => v.AllGroupsToggleButton.IsChecked));
                d(this.OneWayBind(ViewModel, vm => vm.IsShowingAllGroups, v => v.AllGroupsGrid.Visibility));
                d(this.OneWayBind(ViewModel, vm => vm.DirectGroups, v => v.DirectGroupsListView.ItemsSource));
                d(this.Bind(ViewModel, vm => vm.SelectedDirectGroup, v => v.DirectGroupsListView.SelectedItem));
                d(this.Bind(ViewModel, vm => vm.SelectedAllGroup, v => v.AllGroupsListView.SelectedItem));
                d(this.Bind(ViewModel, vm => vm.GroupFilter, v => v.GroupFilterTextBox.Text));
                d(this.Bind(ViewModel, vm => vm.UseFuzzy, v => v.UseFuzzyToggleButton.IsChecked));
                d(this.OneWayBind(ViewModel, vm => vm.AllGroupsCollectionView, v => v.AllGroupsListView.ItemsSource));
                d(this.OneWayBind(ViewModel, vm => vm.IsLoadingGroups, v => v.IsLoadingGroupsProgressBar.Visibility));
                d(this.OneWayBind(ViewModel, vm => vm.IsLoadingGroups, v => v.IsLoadingGroupsProgressBar.IsIndeterminate));
                d(this.OneWayBind(ViewModel, vm => vm.AllGroupsCollectionView.Count, v => v.ShowingCountRun.Text));
                d(this.OneWayBind(ViewModel, vm => vm.AllGroups.Count, v => v.TotalCountRun.Text));

                d(_directGroupsListViewItemDoubleClicks.ToEventCommandSignal().InvokeCommand(ViewModel.FindDirectGroup));
                d(this.BindCommand(ViewModel, vm => vm.FindDirectGroup, v => v.OpenMemberOfMenuItem));
                d(_allGroupsListViewItemDoubleClicks.ToEventCommandSignal().InvokeCommand(ViewModel.FindAllGroup));
                d(this.BindCommand(ViewModel, vm => vm.FindAllGroup, v => v.OpenMemberOfAllMenuItem));
                d(this.BindCommand(ViewModel, vm => vm.OpenEditMemberOf, v => v.EditGroupsButton));
                d(this.BindCommand(ViewModel, vm => vm.SaveDirectGroups, v => v.SaveGroupsButton));
                d(this.BindCommand(ViewModel, vm => vm.SaveAllGroups, v => v.SaveAllGroupsButton));

                d(ViewModel
                .WhenAnyValue(x => x.IsShowingAllGroups)
                .Where(x => x)
                .Select(_ => Unit.Default)
                .ObserveOnDispatcher()
                .InvokeCommand(ViewModel, x => x.GetAllGroups));
            });
        }

        public Interaction<MessageInfo, Unit> InfoMessages => ViewModel.InfoMessages;

        public Interaction<MessageInfo, Unit> ErrorMessages => ViewModel.ErrorMessages;

        public Interaction<DialogInfo, Unit> DialogRequests => ViewModel.DialogRequests;

        public UserObject User { get => (UserObject)GetValue(UserProperty); set => SetValue(UserProperty, value); }
        public static readonly DependencyProperty UserProperty = DependencyProperty.Register(nameof(User), typeof(UserObject), typeof(UserGroups), new PropertyMetadata(null));

        public UserGroupsViewModel ViewModel { get => (UserGroupsViewModel)GetValue(ViewModelProperty); set => SetValue(ViewModelProperty, value); }
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(UserGroupsViewModel), typeof(UserGroups), new PropertyMetadata(null));

        object IViewFor.ViewModel { get => ViewModel; set => ViewModel = value as UserGroupsViewModel; }

        private Subject<MouseButtonEventArgs> _directGroupsListViewItemDoubleClicks = new Subject<MouseButtonEventArgs>();
        private void DirectGroupsListViewItem_DoubleClick(object sender, MouseButtonEventArgs e) => _directGroupsListViewItemDoubleClicks.OnNext(e);

        private Subject<MouseButtonEventArgs> _allGroupsListViewItemDoubleClicks = new Subject<MouseButtonEventArgs>();
        private void AllGroupsListViewItem_DoubleClick(object sender, MouseButtonEventArgs e) => _allGroupsListViewItemDoubleClicks.OnNext(e);
    }
}
