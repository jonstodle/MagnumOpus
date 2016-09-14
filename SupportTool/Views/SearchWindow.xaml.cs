using ReactiveUI;
using SupportTool.Helpers;
using SupportTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
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
	/// Interaction logic for SearchWindow.xaml
	/// </summary>
	public partial class SearchWindow : Window, IViewFor<SearchWindowViewModel>
	{
		public SearchWindow()
		{
			InitializeComponent();

			this.Bind(ViewModel, vm => vm.SearchQuery, v => v.SearchQueryTextBox.Text);
			this.OneWayBind(ViewModel, vm => vm.SearchResultsView, v => v.SearchResultsListView.ItemsSource);
			this.Bind(ViewModel, vm => vm.SelectedSearchResult, v => v.SearchResultsListView.SelectedItem);

			this.WhenActivated(d =>
			{
				SearchQueryTextBox.Focus();
				SearchQueryTextBox.SelectionStart = ViewModel.SearchQuery.Length;

				d(ViewModel
					.WhenAnyValue(x => x.SearchQuery)
					.Throttle(TimeSpan.FromMilliseconds(500))
					.Where(x => x.HasValue())
					.Select(_ => Unit.Default)
					.ObserveOnDispatcher()
					.InvokeCommand(ViewModel, x => x.Search));
				d(this.BindCommand(ViewModel, vm => vm.Choose, v => v.SearchResultsListView, nameof(ListView.MouseDoubleClick)));
				d(this.BindCommand(ViewModel, vm => vm.Choose, v => v.ChooseButton));
			});
		}

		public SearchWindowViewModel ViewModel
		{
			get { return (SearchWindowViewModel)GetValue(ViewModelProperty); }
			set { SetValue(ViewModelProperty, value); }
		}

		public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(SearchWindowViewModel), typeof(SearchWindow), new PropertyMetadata(new SearchWindowViewModel()));

		object IViewFor.ViewModel
		{
			get { return ViewModel; }
			set { ViewModel = value as SearchWindowViewModel; }
		}
	}
}
