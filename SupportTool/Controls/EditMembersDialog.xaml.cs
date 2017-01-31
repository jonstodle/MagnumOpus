using ReactiveUI;
using SupportTool.Models;
using SupportTool.ViewModels;
using System;
using System.Linq;
using System.Reactive.Linq;
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
    /// Interaction logic for EditMembersDialog.xaml
    /// </summary>
    public partial class EditMembersDialog : UserControl, IViewFor<EditMembersDialogViewModel>
	{
		public EditMembersDialog()
		{
			InitializeComponent();

			ViewModel = new EditMembersDialogViewModel();

			this.OneWayBind(ViewModel, vm => vm.Group, v => v.TitleTextBlock.Text, x => x != null ? $"Edit {x.Principal.Name}'s Members" : "");

			this.WhenActivated(d =>
			{
                d(this.Bind(ViewModel, vm => vm.SearchQuery, v => v.SearchQueryTextBox.Text));
                d(this.OneWayBind(ViewModel, vm => vm.SearchResults, v => v.SearchResultsListView.ItemsSource));
                d(this.Bind(ViewModel, vm => vm.SelectedSearchResult, v => v.SearchResultsListView.SelectedItem));
                d(this.OneWayBind(ViewModel, vm => vm.GroupMembers, v => v.GroupMembersListView.ItemsSource));
                d(this.Bind(ViewModel, vm => vm.SelectedGroupMember, v => v.GroupMembersListView.SelectedItem));

                SearchQueryTextBox.Focus();
				d(ViewModel
					.WhenAnyValue(x => x.Group)
					.WhereNotNull()
					.SubscribeOnDispatcher()
					.ToSignal()
					.InvokeCommand(ViewModel, x => x.GetGroupMembers));
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
				d(SearchResultsListView.Events()
					.MouseDoubleClick
					.ToSignal()
					.InvokeCommand(ViewModel, x => x.AddToGroup));
                d(this.BindCommand(ViewModel, vm => vm.AddToGroup, v => v.AddMenuItem));
                d(this.BindCommand(ViewModel, vm => vm.OpenSearchResult, v => v.OpenSearchResultMenuItem, nameof(MenuItem.Click)));
				d(GroupMembersListView.Events()
					.MouseDoubleClick
					.ToSignal()
					.InvokeCommand(ViewModel, x => x.RemoveFromGroup));
                d(this.BindCommand(ViewModel, vm => vm.RemoveFromGroup, v => v.RemoveMenuItem));
                d(this.BindCommand(ViewModel, vm => vm.OpenGroupMember, v => v.OpenGroupMemberMenuItem, nameof(MenuItem.Click)));
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

		public EditMembersDialogViewModel ViewModel
		{
			get { return (EditMembersDialogViewModel)GetValue(ViewModelProperty); }
			set { SetValue(ViewModelProperty, value); }
		}

		public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(EditMembersDialogViewModel), typeof(EditMembersDialog), new PropertyMetadata(null));

		object IViewFor.ViewModel
		{
			get { return ViewModel; }
			set { ViewModel = value as EditMembersDialogViewModel; }
		}
	}
}
