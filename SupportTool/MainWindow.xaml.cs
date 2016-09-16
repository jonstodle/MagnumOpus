using ReactiveUI;
using SupportTool.ViewModels;
using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace SupportTool
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window, IViewFor<MainWindowViewModel>
	{
		public MainWindow()
		{
			SupportTool.Services.NavigationServices.NavigationService.Init(this);

			InitializeComponent();

			this.Bind(ViewModel, vm => vm.SearchQuery, v => v.SearchQueryTextBox.Text);
			this.OneWayBind(ViewModel, vm => vm.History, v => v.HistoryButtonContextMenu.ItemsSource);
			this.OneWayBind(ViewModel, vm => vm.SearchResultsView, v => v.SearchResultsListView.ItemsSource);
			this.Bind(ViewModel, vm => vm.SelectedSearchResult, v => v.SearchResultsListView.SelectedItem);

			this.WhenActivated(d =>
			{
				SearchQueryTextBox.Focus();
				SearchQueryTextBox.SelectAll();

				d(this.BindCommand(ViewModel, vm => vm.Paste, v => v.PasteButton));
				d(Observable.Merge(
					SearchQueryTextBox.Events()
						.KeyDown
						.Where(x => x.Key == Key.Enter)
						.Select(_ => ViewModel.SearchQuery),
					ViewModel
						.WhenAnyValue(x => x.SearchQuery)
						.Where(x => !x.IsIPAddress())
						.Throttle(TimeSpan.FromSeconds(1)))
					.DistinctUntilChanged()
					.Where(x => x.HasValue(3))
					.Select(_ => Unit.Default)
					.ObserveOnDispatcher()
					.InvokeCommand(ViewModel, x => x.Search));
				d(SearchResultsListView.Events()
					.MouseDoubleClick
					.Select(_ => Unit.Default)
					.InvokeCommand(ViewModel, x => x.Open));
				d(Observable.FromEventPattern(HistoryButton, nameof(Button.Click))
					.Subscribe(e =>
					{
						HistoryButtonContextMenu.PlacementTarget = e.Sender as Button;
						HistoryButtonContextMenu.IsOpen = true;
					}));
			});
		}

		private void MenuItemClick(object sender, RoutedEventArgs args)
		{
			var menuItem = sender as MenuItem;
			var header = menuItem.Header as string;
			ViewModel.SearchQuery = header;
		}

		public MainWindowViewModel ViewModel
		{
			get { return (MainWindowViewModel)GetValue(ViewModelProperty); }
			set { SetValue(ViewModelProperty, value); }
		}

		public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(MainWindowViewModel), typeof(MainWindow), new PropertyMetadata(new MainWindowViewModel()));

		object IViewFor.ViewModel
		{
			get { return ViewModel; }
			set { ViewModel = value as MainWindowViewModel; }
		}
	}
}
