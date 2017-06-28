using ReactiveUI;
using MagnumOpus.ViewModels;
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
using MagnumOpus.Services.ActiveDirectoryServices;
using MagnumOpus.Views;
using System.Reactive.Disposables;

namespace MagnumOpus
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : WindowBase<MainWindowViewModel>
    {
        public MainWindow()
        {
            MagnumOpus.Services.NavigationServices.NavigationService.Init(this);

            InitializeComponent();

            ViewModel = new MainWindowViewModel();

            this.Events()
                .Activated
                .Subscribe(_ => SearchQueryTextBox.Focus());

            this.WhenActivated(d =>
            {
                MainGrid.IsEnabled = ActiveDirectoryService.IsInDomain();
                NoDomainStackPanel.Visibility = ActiveDirectoryService.IsInDomain() ? Visibility.Collapsed : Visibility.Visible;

                if (ActiveDirectoryService.IsInDomain())
                {
                    this.Bind(ViewModel, vm => vm.SearchQuery, v => v.SearchQueryTextBox.Text).DisposeWith(d);
                    this.OneWayBind(ViewModel, vm => vm.History, v => v.HistoryButtonContextMenu.ItemsSource).DisposeWith(d);
                    this.OneWayBind(ViewModel, vm => vm.SearchResults, v => v.SearchResultsListView.ItemsSource).DisposeWith(d);
                    this.Bind(ViewModel, vm => vm.SelectedSearchResult, v => v.SearchResultsListView.SelectedItem).DisposeWith(d);
                    this.OneWayBind(ViewModel, vm => vm.IsNoResults, v => v.NoResultsTextBlock.Visibility).DisposeWith(d);
                    this.OneWayBind(ViewModel, vm => vm.SearchResults.Count, v => v.SearchResultsCountTextBox.Text, count => $"{count} {(count == 1 ? "result" : "results")}").DisposeWith(d);
                    this.OneWayBind(ViewModel, vm => vm.Domain, v => v.DomainTextBlock.Text).DisposeWith(d);

                    this.BindCommand(ViewModel, vm => vm.Paste, v => v.PasteButton).DisposeWith(d);
                    Observable.Merge(
                            SearchQueryTextBox.Events()
                                .KeyDown
                                .Where(args => args.Key == Key.Enter)
                                .Select(_ => ViewModel.SearchQuery),
                            ViewModel
                                .Paste
                                .Select(_ => ViewModel.SearchQuery),
                            ViewModel
                                .WhenAnyValue(vm => vm.SearchQuery)
                                .Where(searchQuery => searchQuery.HasValue(3) && !int.TryParse(searchQuery.First().ToString(), out int i))
                                .Throttle(TimeSpan.FromSeconds(1)))
                        .DistinctUntilChanged()
                        .Where(searchQuery => searchQuery.HasValue(3))
                        .ToSignal()
                        .ObserveOnDispatcher()
                        .InvokeCommand(ViewModel.Search)
                        .DisposeWith(d);
                    SearchResultsListView.Events()
                        .MouseDoubleClick
                        .ToSignal()
                        .InvokeCommand(ViewModel.Open)
                        .DisposeWith(d);
                    this.BindCommand(ViewModel, vm => vm.Open, v => v.OpenSearchResultsMenuItem).DisposeWith(d);
                    Observable.FromEventPattern(HistoryButton, nameof(Button.Click))
                        .Subscribe(e =>
                        {
                            HistoryButtonContextMenu.PlacementTarget = e.Sender as Button;
                            HistoryButtonContextMenu.IsOpen = true;
                        })
                        .DisposeWith(d);
                    this.BindCommand(ViewModel, vm => vm.OpenSettings, v => v.SettingsButton).DisposeWith(d);
                    this.Events()
                        .Closed
                        .Subscribe(_ => Application.Current.Shutdown())
                        .DisposeWith(d);
                    SearchQueryTextBox.Events().GotFocus
                        .Subscribe(_ => SearchQueryTextBox.SelectAll())
                        .DisposeWith(d);
                    SearchQueryTextBox.Events().KeyUp
                        .Where(args => args.Key == Key.Down && SearchResultsListView.Items.Count > 0)
                        .Subscribe(_ => { SearchResultsListView.SelectedIndex = 0; Keyboard.Focus(SearchResultsListView.ItemContainerGenerator.ContainerFromIndex(0) as IInputElement); })
                        .DisposeWith(d);
                    SearchResultsListView.Events().KeyDown
                        .Where(args => args.Key == Key.Up && SearchResultsListView.SelectedIndex == 0)
                        .Subscribe(_ => Keyboard.Focus(SearchQueryTextBox))
                        .DisposeWith(d);
                    SearchResultsListView.Events().KeyDown
                        .Where(args => args.Key == Key.Enter)
                        .ToSignal()
                        .InvokeCommand(ViewModel.Open)
                        .DisposeWith(d);
                    this.Events().KeyDown
                        .Where(args => args.Key == Key.F3)
                        .Subscribe(_ => Keyboard.Focus(SearchQueryTextBox))
                        .DisposeWith(d);
                }
            });
        }



        private void MenuItemClick(object sender, RoutedEventArgs args)
        {
            var menuItem = sender as MenuItem;
            var header = menuItem.Header as string;
            ViewModel.SearchQuery = header;
        }
    }
}
