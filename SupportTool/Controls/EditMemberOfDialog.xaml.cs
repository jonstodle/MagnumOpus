using ReactiveUI;
using SupportTool.Models;
using SupportTool.ViewModels;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace SupportTool.Controls
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
                d(this.OneWayBind(ViewModel, vm => vm.Principal, v => v.TitleTextBlock.Text, x => x != null ? $"Edit {x.Name}'s MemberOf" : ""));

                d(this.Bind(ViewModel, vm => vm.SearchQuery, v => v.SearchQueryTextBox.Text));
                d(this.OneWayBind(ViewModel, vm => vm.SearchResults, v => v.SearchResultsListView.ItemsSource));
                d(this.Bind(ViewModel, vm => vm.SelectedSearchResult, v => v.SearchResultsListView.SelectedItem));
                d(this.OneWayBind(ViewModel, vm => vm.PrincipalMembers, v => v.PrincipalMembersListView.ItemsSource));
                d(this.Bind(ViewModel, vm => vm.SelectedPrincipalMember, v => v.PrincipalMembersListView.SelectedItem));

                SearchQueryTextBox.Focus();
				d(ViewModel
					.WhenAnyValue(x => x.Principal)
					.WhereNotNull()
					.SubscribeOnDispatcher()
					.ToSignal()
					.InvokeCommand(ViewModel, x => x.GetPrincipalMembers));
				d(Observable.Merge(
						SearchQueryTextBox.Events()
							.KeyDown
							.Where(x => x.Key == Key.Enter)
							.Select(_ => ViewModel.SearchQuery),
						ViewModel
							.WhenAnyValue(x => x.SearchQuery)
							.Throttle(TimeSpan.FromSeconds(1)))
					.Where(x => x.HasValue(3))
					.DistinctUntilChanged()
					.SubscribeOnDispatcher()
					.ToSignal()
					.InvokeCommand(ViewModel, x => x.Search));
                d(this.BindCommand(ViewModel, vm => vm.AddToPrincipal, v => v.AddMenuItem));
                d(this.BindCommand(ViewModel, vm => vm.OpenSearchResultPrincipal, v => v.OpenSearchResultMenuItem));
                d(_searchResultsListViewItemDoubleClick.ToEventCommandSignal().InvokeCommand(ViewModel.AddToPrincipal));
                d(this.BindCommand(ViewModel, vm => vm.RemoveFromPrincipal, v => v.RemoveMenuItem));
                d(this.BindCommand(ViewModel, vm => vm.OpenMembersPrincipal, v => v.OpenMembersPrincipalMenuItem));
                d(_principalMembersListViewItemDoubleClick.ToEventCommandSignal().InvokeCommand(ViewModel.RemoveFromPrincipal));
				d(this.BindCommand(ViewModel, vm => vm.Save, v => v.SaveButton));
                d(this.BindCommand(ViewModel, vm => vm.Cancel, v => v.CancelButton));
				d(ViewModel
					.InfoMessages
					.RegisterInfoHandler(ContainerGrid));
				d(ViewModel
					.ErrorMessages
					.RegisterErrorHandler(ContainerGrid));
			});
		}

		public EditMemberOfDialogViewModel ViewModel
		{
			get { return (EditMemberOfDialogViewModel)GetValue(ViewModelProperty); }
			set { SetValue(ViewModelProperty, value); }
		}

		public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(EditMemberOfDialogViewModel), typeof(EditMemberOfDialog), new PropertyMetadata(null));

		object IViewFor.ViewModel
		{
			get { return ViewModel; }
			set { ViewModel = value as EditMemberOfDialogViewModel; }
		}

        private Subject<MouseButtonEventArgs> _searchResultsListViewItemDoubleClick = new Subject<MouseButtonEventArgs>();
        private void SearchResultsListViewItem_DoubleClick(object sender, MouseButtonEventArgs e) => _searchResultsListViewItemDoubleClick.OnNext(e);

        private Subject<MouseButtonEventArgs> _principalMembersListViewItemDoubleClick = new Subject<MouseButtonEventArgs>();
        private void PrincipalMembersListViewItem_DoubleClick(object sender, MouseButtonEventArgs e) => _principalMembersListViewItemDoubleClick.OnNext(e);
    }
}
