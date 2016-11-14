using ReactiveUI;
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

namespace SupportTool.Views
{
	/// <summary>
	/// Interaction logic for EditMembers.xaml
	/// </summary>
	public partial class EditMembersWindow : Window, IViewFor<EditMembersWindowViewModel>
	{
		public EditMembersWindow()
		{
			InitializeComponent();

			ViewModel = new EditMembersWindowViewModel();

			this.OneWayBind(ViewModel, vm => vm.Group, v => v.Title, x => x != null ? $"Edit {x.Principal.Name}'s Members" : "");

			this.Bind(ViewModel, vm => vm.SearchQuery, v => v.SearchQueryTextBox.Text);
			this.OneWayBind(ViewModel, vm => vm.SearchResultsView, v => v.SearchResultsListView.ItemsSource);
			this.Bind(ViewModel, vm => vm.SelectedSearchResult, v => v.SearchResultsListView.SelectedItem);
			this.OneWayBind(ViewModel, vm => vm.GroupMembersView, v => v.GroupMembersListView.ItemsSource);
			this.Bind(ViewModel, vm => vm.SelectedGroupMember, v => v.GroupMembersListView.SelectedItem);

			this.WhenActivated(d =>
			{
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
				d(GroupMembersListView.Events()
					.MouseDoubleClick
					.ToSignal()
					.InvokeCommand(ViewModel, x => x.RemoveFromGroup));
				d(this.BindCommand(ViewModel, vm => vm.Save, v => v.SaveButton));
				d(ViewModel
					.InfoMessages
					.RegisterInfoHandler(ContainerGrid));
				d(ViewModel
					.ErrorMessages
					.RegisterErrorHandler(ContainerGrid));
			});
		}

		public EditMembersWindowViewModel ViewModel
		{
			get { return (EditMembersWindowViewModel)GetValue(ViewModelProperty); }
			set { SetValue(ViewModelProperty, value); }
		}

		public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(EditMembersWindowViewModel), typeof(EditMembersWindow), new PropertyMetadata(null));

		object IViewFor.ViewModel
		{
			get { return ViewModel; }
			set { ViewModel = value as EditMembersWindowViewModel; }
		}
	}
}
