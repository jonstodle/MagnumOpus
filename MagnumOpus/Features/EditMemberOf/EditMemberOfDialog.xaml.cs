using ReactiveUI;
using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Reactive.Disposables;
using MagnumOpus.Dialog;

namespace MagnumOpus.EditMemberOf
{
    /// <summary>
    /// Interaction logic for EditMemberOfDialog.xaml
    /// </summary>
    public partial class EditMemberOfDialog : UserControl, IViewFor<EditMemberOfDialogViewModel>
    {
        public EditMemberOfDialog()
        {
            InitializeComponent();

            ViewModel = new EditMemberOfDialogViewModel();

            this.WhenActivated(d =>
            {
                this.OneWayBind(ViewModel, vm => vm.Principal, v => v.TitleTextBlock.Text, principal => principal != null ? $"Edit {principal.Name}'s MemberOf" : "").DisposeWith(d);

                this.Bind(ViewModel, vm => vm.SearchQuery, v => v.SearchQueryTextBox.Text).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.SearchResults, v => v.SearchResultsListView.ItemsSource).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.SelectedSearchResult, v => v.SearchResultsListView.SelectedItem).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.PrincipalMembers, v => v.PrincipalMembersListView.ItemsSource).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.SelectedPrincipalMember, v => v.PrincipalMembersListView.SelectedItem).DisposeWith(d);

                SearchQueryTextBox.Focus();
                ViewModel
                    .WhenAnyValue(vm => vm.Principal)
                    .WhereNotNull()
                    .SubscribeOnDispatcher()
                    .ToSignal()
                    .InvokeCommand(ViewModel.GetPrincipalMembers)
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
                this.BindCommand(ViewModel, vm => vm.AddToPrincipal, v => v.AddMenuItem).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.OpenSearchResultPrincipal, v => v.OpenSearchResultMenuItem).DisposeWith(d);
                _searchResultsListViewItemDoubleClick.ToEventCommandSignal().InvokeCommand(ViewModel.AddToPrincipal).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.RemoveFromPrincipal, v => v.RemoveMenuItem).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.OpenMembersPrincipal, v => v.OpenMembersPrincipalMenuItem).DisposeWith(d);
                _principalMembersListViewItemDoubleClick.ToEventCommandSignal().InvokeCommand(ViewModel.RemoveFromPrincipal).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.Save, v => v.SaveButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.Cancel, v => v.CancelButton).DisposeWith(d);
                ViewModel
                    .Messages
                    .RegisterMessageHandler(ContainerGrid)
                    .DisposeWith(d);
            });
        }

        public EditMemberOfDialogViewModel ViewModel { get => (EditMemberOfDialogViewModel)GetValue(ViewModelProperty); set => SetValue(ViewModelProperty, value); }
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(EditMemberOfDialogViewModel), typeof(EditMemberOfDialog), new PropertyMetadata(null));

        object IViewFor.ViewModel { get => ViewModel; set => ViewModel = value as EditMemberOfDialogViewModel; }

        private Subject<MouseButtonEventArgs> _searchResultsListViewItemDoubleClick = new Subject<MouseButtonEventArgs>();
        private void SearchResultsListViewItem_DoubleClick(object sender, MouseButtonEventArgs e) => _searchResultsListViewItemDoubleClick.OnNext(e);

        private Subject<MouseButtonEventArgs> _principalMembersListViewItemDoubleClick = new Subject<MouseButtonEventArgs>();
        private void PrincipalMembersListViewItem_DoubleClick(object sender, MouseButtonEventArgs e) => _principalMembersListViewItemDoubleClick.OnNext(e);
    }
}
