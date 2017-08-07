using ReactiveUI;
using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Reactive.Disposables;
using MagnumOpus.Dialog;

namespace MagnumOpus.EditMembers
{
    /// <summary>
    /// Interaction logic for EditMembersDialog.xaml
    /// </summary>
    public partial class EditMembersDialog : UserControl, IViewFor<EditMembersDialogViewModel>
    {
        public EditMembersDialog()
        {
            InitializeComponent();

            ViewModel = new EditMembersDialogViewModel();

            this.OneWayBind(ViewModel, vm => vm.Group, v => v.TitleTextBlock.Text, group => group != null ? $"Edit {group.Principal.Name}'s Members" : "");

            this.WhenActivated(d =>
            {
                this.Bind(ViewModel, vm => vm.SearchQuery, v => v.SearchQueryTextBox.Text).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.SearchResults, v => v.SearchResultsListView.ItemsSource).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.SelectedSearchResult, v => v.SearchResultsListView.SelectedItem).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.GroupMembers, v => v.GroupMembersListView.ItemsSource).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.SelectedGroupMember, v => v.GroupMembersListView.SelectedItem).DisposeWith(d);

                SearchQueryTextBox.Focus();
                ViewModel
                    .WhenAnyValue(vm => vm.Group)
                    .WhereNotNull()
                    .SubscribeOnDispatcher()
                    .ToSignal()
                    .InvokeCommand(ViewModel.GetGroupMembers)
                    .DisposeWith(d);
                Observable.Merge(
                        SearchQueryTextBox.Events()
                            .KeyDown
                            .Where(args => args.Key == Key.Enter)
                            .Select(_ => ViewModel.SearchQuery),
                        ViewModel
                            .WhenAnyValue(vm => vm.SearchQuery)
                            .Throttle(TimeSpan.FromSeconds(1)))
                    .Where(searchQuery => searchQuery.HasValue(3))
                    .DistinctUntilChanged()
                    .SubscribeOnDispatcher()
                    .ToSignal()
                    .InvokeCommand(ViewModel.Search)
                    .DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.AddToGroup, v => v.AddMenuItem).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.OpenSearchResult, v => v.OpenSearchResultMenuItem).DisposeWith(d);
                _searchResultsListViewItemDoubleClick.ToEventCommandSignal().InvokeCommand(ViewModel.AddToGroup).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.RemoveFromGroup, v => v.RemoveMenuItem).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.OpenGroupMember, v => v.OpenGroupMemberMenuItem).DisposeWith(d);
                _groupMembersListViewItemDoubleClick.ToEventCommandSignal().InvokeCommand(ViewModel.RemoveFromGroup).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.Save, v => v.SaveButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.Cancel, v => v.CancelButton).DisposeWith(d);
                ViewModel
                    .Messages
                    .RegisterMessageHandler(ContainerGrid)
                    .DisposeWith(d);
            });
        }

        public EditMembersDialogViewModel ViewModel { get => (EditMembersDialogViewModel)GetValue(ViewModelProperty); set => SetValue(ViewModelProperty, value); }
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(EditMembersDialogViewModel), typeof(EditMembersDialog), new PropertyMetadata(null));

        object IViewFor.ViewModel { get => ViewModel; set => ViewModel = value as EditMembersDialogViewModel; }

        private Subject<MouseButtonEventArgs> _searchResultsListViewItemDoubleClick = new Subject<MouseButtonEventArgs>();
        private void SearchResultsListViewItem_DoubleClick(object sender, MouseButtonEventArgs e) => _searchResultsListViewItemDoubleClick.OnNext(e);

        private Subject<MouseButtonEventArgs> _groupMembersListViewItemDoubleClick = new Subject<MouseButtonEventArgs>();
        private void GroupMembersListViewItem_DoubleClick(object sender, MouseButtonEventArgs e) => _groupMembersListViewItemDoubleClick.OnNext(e);
    }
}
