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

namespace MagnumOpus
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IViewFor<MainWindowViewModel>
    {
        public MainWindow()
        {
            MagnumOpus.Services.NavigationServices.NavigationService.Init(this);

            InitializeComponent();

            ViewModel = new MainWindowViewModel();

            this.Events()
                .Activated
                .Subscribe(_ =>
                {
                    SearchQueryTextBox.Focus();
                    SearchQueryTextBox.SelectAll();
                });

            this.WhenActivated(d =>
            {
                if(ActiveDirectoryService.Current.CurrentDomain == null)
                {
                    MainGrid.IsEnabled = false;
                    NoDomainStackPanel.Visibility = Visibility.Visible;
                }
                else
                {
                    MainGrid.IsEnabled = true;
                    NoDomainStackPanel.Visibility = Visibility.Collapsed;
                }

                d(this.Bind(ViewModel, vm => vm.SearchQuery, v => v.SearchQueryTextBox.Text));
                d(this.OneWayBind(ViewModel, vm => vm.History, v => v.HistoryButtonContextMenu.ItemsSource));
                d(this.OneWayBind(ViewModel, vm => vm.SearchResults, v => v.SearchResultsListView.ItemsSource));
                d(this.Bind(ViewModel, vm => vm.SelectedSearchResult, v => v.SearchResultsListView.SelectedItem));
                d(this.OneWayBind(ViewModel, vm => vm.IsNoResults, v => v.NoResultsTextBlock.Visibility));
                d(this.OneWayBind(ViewModel, vm => vm.SearchResults.Count, v => v.SearchResultsCountTextBox.Text, x => $"{x} {(x == 1 ? "result" : "results")}"));
                d(this.OneWayBind(ViewModel, vm => vm.ShowVersion, v => v.SearchResultsStackPanel.Opacity, x => x ? 0 : 1));
                d(this.OneWayBind(ViewModel, vm => vm.ShowVersion, v => v.VersionTextBlock.Opacity, x => x ? 1 : 0));
                d(this.OneWayBind(ViewModel, vm => vm.Version, v => v.VersionTextBlock.Text));
                d(this.OneWayBind(ViewModel, vm => vm.Domain, v => v.DomainTextBlock.Text));

                d(this.BindCommand(ViewModel, vm => vm.Paste, v => v.PasteButton));
                d(Observable.Merge(
                    SearchQueryTextBox.Events()
                        .KeyDown
                        .Where(x => x.Key == Key.Enter)
                        .Select(_ => ViewModel.SearchQuery),
                    ViewModel
                        .Paste
                        .Select(_ => ViewModel.SearchQuery),
                    ViewModel
                        .WhenAnyValue(x => x.SearchQuery)
                        .Where(x => x.Length > 0 && !int.TryParse(x.First().ToString(), out int i))
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
                d(this.BindCommand(ViewModel, vm => vm.Open, v => v.OpenSearchResultsMenuItem));
                d(Observable.FromEventPattern(HistoryButton, nameof(Button.Click))
                    .Subscribe(e =>
                    {
                        HistoryButtonContextMenu.PlacementTarget = e.Sender as Button;
                        HistoryButtonContextMenu.IsOpen = true;
                    }));
                d(this.BindCommand(ViewModel, vm => vm.OpenSettings, v => v.SettingsButton));
                d(SearchResultsStackPanel.Events()
                    .MouseDown
                    .ToSignal()
                    .InvokeCommand(ViewModel.ToggleShowVersion));
                d(this.Events()
                    .Closed
                    .Subscribe(_ => Application.Current.Shutdown()));
            });
        }



        public MainWindowViewModel ViewModel { get => (MainWindowViewModel)GetValue(ViewModelProperty); set => SetValue(ViewModelProperty, value); }
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(MainWindowViewModel), typeof(MainWindow), new PropertyMetadata(null));

        object IViewFor.ViewModel { get => ViewModel; set => ViewModel = value as MainWindowViewModel; }



        private void MenuItemClick(object sender, RoutedEventArgs args)
        {
            var menuItem = sender as MenuItem;
            var header = menuItem.Header as string;
            ViewModel.SearchQuery = header;
        }
    }
}
