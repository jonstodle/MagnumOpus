using ReactiveUI;
using SupportTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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

			this.Bind(ViewModel, vm => vm.SearchQuery, v => v.SearchQueryTextBox.Text);
			this.OneWayBind(ViewModel, vm => vm.SearchResultsView, v => v.SearchResultsListView.ItemsSource);
			this.Bind(ViewModel, vm => vm.SelectedSearchResult, v => v.SearchResultsListView.SelectedItem);
			this.OneWayBind(ViewModel, vm => vm.PrincipalMembersView, v => v.PrincipalMembersListView.ItemsSource);
			this.Bind(ViewModel, vm => vm.SelectedPrincipalMember, v => v.PrincipalMembersListView.SelectedItem);

			this.WhenActivated(d =>
			{
				SearchQueryTextBox.Focus();
				d(Observable.Merge(
						SearchQueryTextBox.Events()
							.KeyDown
							.Where(x => x.Key == Key.Enter)
							.Select(_ => ViewModel.SearchQuery),
						ViewModel
							.WhenAnyValue(x => x.SearchQuery)
							.Throttle(TimeSpan.FromSeconds(1)))
					.DistinctUntilChanged()
					.SubscribeOnDispatcher()
					.InvokeCommand(ViewModel, x => x.Search));
				d(SearchResultsListView.Events()
					.MouseDoubleClick
					.ToSignal()
					.InvokeCommand(ViewModel, x => x.AddToPrincipal));
				d(PrincipalMembersListView.Events()
					.MouseDoubleClick
					.ToSignal()
					.InvokeCommand(ViewModel, x => x.RemoveFromPrincipal));
				d(this.BindCommand(ViewModel, vm => vm.Search, v => v.SaveButton));
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
