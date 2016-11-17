using ReactiveUI;
using SupportTool.Models;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

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

			//this.OneWayBind(ViewModel, vm => vm.Principal, v => v.Title, x => x != null ? $"Edit {x.Name}'s MemberOf" : "");

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
	}
}
