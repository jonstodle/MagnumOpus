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
using System.Reactive.Disposables;

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
                this.Bind(ViewModel, vm => vm.User, v => v.User).DisposeWith(d);

                this.Bind(ViewModel, vm => vm.IsShowingDirectGroups, v => v.DirectGroupsToggleButton.IsChecked).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.IsShowingDirectGroups, v => v.DirectGroupsGrid.Visibility).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.IsShowingAllGroups, v => v.AllGroupsToggleButton.IsChecked).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.IsShowingAllGroups, v => v.AllGroupsGrid.Visibility).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.DirectGroups, v => v.DirectGroupsListView.ItemsSource).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.SelectedDirectGroup, v => v.DirectGroupsListView.SelectedItem).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.SelectedAllGroup, v => v.AllGroupsListView.SelectedItem).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.GroupFilter, v => v.GroupFilterTextBox.Text).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.UseFuzzy, v => v.UseFuzzyToggleButton.IsChecked).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.AllGroupsCollectionView, v => v.AllGroupsListView.ItemsSource).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.IsLoadingGroups, v => v.IsLoadingGroupsProgressBar.Visibility).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.IsLoadingGroups, v => v.IsLoadingGroupsProgressBar.IsIndeterminate).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.AllGroupsCollectionView.Count, v => v.ShowingCountRun.Text).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.AllGroups.Count, v => v.TotalCountRun.Text).DisposeWith(d);

                _directGroupsListViewItemDoubleClicks.ToEventCommandSignal().InvokeCommand(ViewModel.FindDirectGroup).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.FindDirectGroup, v => v.OpenMemberOfMenuItem).DisposeWith(d);
                _allGroupsListViewItemDoubleClicks.ToEventCommandSignal().InvokeCommand(ViewModel.FindAllGroup).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.FindAllGroup, v => v.OpenMemberOfAllMenuItem).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.OpenEditMemberOf, v => v.EditGroupsButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.SaveDirectGroups, v => v.SaveGroupsButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.SaveAllGroups, v => v.SaveAllGroupsButton).DisposeWith(d);

                ViewModel
                .WhenAnyValue(x => x.IsShowingAllGroups)
                .Where(x => x)
                .Select(_ => Unit.Default)
                .ObserveOnDispatcher()
                .InvokeCommand(ViewModel, x => x.GetAllGroups).DisposeWith(d);
            });
        }

        public Interaction<MessageInfo, int> Messages => ViewModel.Messages;

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
