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
	/// Interaction logic for EditMemberOfWindow.xaml
	/// </summary>
	public partial class EditMemberOfWindow : Window, IViewFor<EditMemberOfWindowViewModel>
	{
		public EditMemberOfWindow()
		{
			InitializeComponent();

			ViewModel = new EditMemberOfWindowViewModel();

			this.OneWayBind(ViewModel, vm => vm.Principal, v => v.Title, x => x != null ? $"Edit {x.Name}'s MemberOf" : "");

			this.Bind(ViewModel, vm => vm.SearchQuery, v => v.SearchQueryTextBox.Text);
			this.OneWayBind(ViewModel, vm => vm.SearchResultsView, v => v.SearchResultsListView.ItemsSource);
			this.Bind(ViewModel, vm => vm.SelectedSearchResult, v => v.SearchResultsListView.SelectedItem);
			this.OneWayBind(ViewModel, vm => vm.PrincipalMembersView, v => v.PrincipalMembersListView.ItemsSource);
			this.Bind(ViewModel, vm => vm.SelectedPrincipalMember, v => v.PrincipalMembersListView.SelectedItem);

			this.WhenActivated(d =>
			{
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
				d(SearchResultsListView.Events()
					.MouseDoubleClick
					.ToSignal()
					.InvokeCommand(ViewModel, x => x.AddToPrincipal));
				d(PrincipalMembersListView.Events()
					.MouseDoubleClick
					.ToSignal()
					.InvokeCommand(ViewModel, x => x.RemoveFromPrincipal));
				d(this.BindCommand(ViewModel, vm => vm.Save, v => v.SaveButton));
			});
		}

		public EditMemberOfWindowViewModel ViewModel
		{
			get { return (EditMemberOfWindowViewModel)GetValue(ViewModelProperty); }
			set { SetValue(ViewModelProperty, value); }
		}

		public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(EditMemberOfWindowViewModel), typeof(EditMemberOfWindow), new PropertyMetadata(null));

		object IViewFor.ViewModel
		{
			get { return ViewModel; }
			set { ViewModel = value as EditMemberOfWindowViewModel; }
		}
	}
}
