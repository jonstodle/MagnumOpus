using ReactiveUI;
using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using MagnumOpus.ActiveDirectory;

namespace MagnumOpus
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : WindowBase<MainWindowViewModel>
    {
        public MainWindow()
        {
            Navigation.NavigationService.Init(this);

            InitializeComponent();

            ViewModel = new MainWindowViewModel();

            this.Events()
                .Activated
                .Subscribe(_ => SearchQueryTextBox.Focus());

            this.WhenActivated(d =>
            {
                MainGrid.IsEnabled = ADFacade.IsInDomain();
                NoDomainStackPanel.Visibility = ADFacade.IsInDomain() ? Visibility.Collapsed : Visibility.Visible;

                if (ADFacade.IsInDomain())
                {
                    this.Bind(ViewModel, vm => vm.SearchQuery, v => v.SearchQueryTextBox.Text).DisposeWith(d);
                    this.OneWayBind(ViewModel, vm => vm.History, v => v.HistoryButtonContextMenu.ItemsSource).DisposeWith(d);
                    this.OneWayBind(ViewModel, vm => vm.SearchResults, v => v.SearchResultsDataGrid.ItemsSource).DisposeWith(d);
                    this.Bind(ViewModel, vm => vm.SelectedSearchResult, v => v.SearchResultsDataGrid.SelectedItem).DisposeWith(d);
                    this.OneWayBind(ViewModel, vm => vm.IsNoResults, v => v.NoResultsTextBlock.Visibility).DisposeWith(d);
                    this.OneWayBind(ViewModel, vm => vm.SearchResults.Count, v => v.SearchResultsCountTextBox.Text, count => $"{count} {(count == 1 ? "result" : "results")}").DisposeWith(d);
                    this.OneWayBind(ViewModel, vm => vm.Domain, v => v.DomainTextBlock.Text).DisposeWith(d);

                    _openSearchResultInvokes = new Subject<Unit>().DisposeWith(d);
                    _openSearchResultInvokes.InvokeCommand(ViewModel.Open);

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
                        .ToSignal()
                        .ObserveOnDispatcher()
                        .InvokeCommand(ViewModel.Search)
                        .DisposeWith(d);
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
                        .Where(args => args.Key == Key.Down && SearchResultsDataGrid.Items.Count > 0)
                        .Subscribe(_ => { SearchResultsDataGrid.SelectedIndex = 0; (SearchResultsDataGrid.ItemContainerGenerator.ContainerFromIndex(0) as DataGridRow)?.MoveFocus(new TraversalRequest(FocusNavigationDirection.First)); })
                        .DisposeWith(d);
                    SearchResultsDataGrid.Events().PreviewKeyDown
                        .Where(args => args.Key == Key.Up && SearchResultsDataGrid.SelectedIndex == 0)
                        .Do(args => args.Handled = true)
                        .Subscribe(_ => Keyboard.Focus(SearchQueryTextBox))
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

        private void SearchResultsDataGridRow_MouseDoubleClick(object sender, MouseButtonEventArgs e) => _openSearchResultInvokes.OnNext(Unit.Default);

        private void OpenSearchResultsMenuItem_Click(object sender, RoutedEventArgs e) => _openSearchResultInvokes.OnNext(Unit.Default);

        private void DataGridRow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                _openSearchResultInvokes.OnNext(Unit.Default);
            }
        }



        private Subject<Unit> _openSearchResultInvokes;
    }
}
