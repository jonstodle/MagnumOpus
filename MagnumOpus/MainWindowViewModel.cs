using ReactiveUI;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Windows;
using MagnumOpus.ActiveDirectory;
using MagnumOpus.Dialog;
using MagnumOpus.IPAddress;
using MagnumOpus.Navigation;
using MagnumOpus.Settings;
using MagnumOpus.State;
using Splat;

namespace MagnumOpus
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel()
        {
            _searchResults = new ReactiveList<DirectoryEntryInfo>();
            _history = new ReactiveList<string>();

            Search = ReactiveCommand.Create(
                () => _searchQuery.IsIPAddress()
                    ? Observable.FromAsync(() => NavigationService.ShowWindow<IPAddressWindow>(_searchQuery)).SelectMany(_ => Observable.Empty<DirectoryEntryInfo>())
                    : _adFacade.SearchDirectory(_searchQuery.Trim(), TaskPoolScheduler.Default).Take(1000).Select(directoryEntry => new DirectoryEntryInfo(directoryEntry)),
                this.WhenAnyValue(vm => vm.SearchQuery).Select(query => query.HasValue(3)));
            Search
                .Do(_ => _searchResults.Clear())
                .Switch()
                .ObserveOnDispatcher()
                .Subscribe(directoryEntryInfo => _searchResults.Add(directoryEntryInfo));

            Paste = ReactiveCommand.Create(() => { SearchQuery = Clipboard.GetText().Trim().ToUpperInvariant(); });

            Open = ReactiveCommand.CreateFromTask(
                async () =>
                {
                    var principal = await _adFacade.GetPrincipal(_selectedSearchResult.CN);

                    await NavigationService.ShowPrincipalWindow(principal);

                    if (!_history.Contains(_selectedSearchResult.CN)) _history.Insert(0, _selectedSearchResult.CN);
                },
                this.WhenAnyValue(vm => vm.SelectedSearchResult).IsNotNull());

            OpenSettings = ReactiveCommand.CreateFromTask(() => NavigationService.ShowDialog<SettingsWindow>());

            _isNoResults = Search
                .Select(searchResults => Observable.Concat(
                    Observable.Return(false),
                    searchResults.Aggregate(0, (acc, curr) => acc + 1).Select(resultCount => resultCount == 0)))
                .Switch()
                .ObserveOnDispatcher()
                .ToProperty(this, vm => vm.IsNoResults);

            Observable.Merge<(string Title, string Message)>(
                    Search.ThrownExceptions.Select(ex => ("Could not complete search", ex.Message)),
                    Paste.ThrownExceptions.Select(ex => ("Could not not paste text", ex.Message)),
                    Open.ThrownExceptions.Select(ex => ("Could not open AD object", ex.Message)),
                    OpenSettings.ThrownExceptions.Select(ex => ("Could not open settings", ex.Message)))
                .SelectMany(dialogContent => _messages.Handle(new MessageInfo(MessageType.Error, dialogContent.Message, dialogContent.Title)))
                .Subscribe();

            StateService.Get(nameof(_history), Enumerable.Empty<string>())
                .SubscribeOn(TaskPoolScheduler.Default)
                .ObserveOnDispatcher()
                .Subscribe(historyList => { using (_history.SuppressChangeNotifications()) _history.AddRange(historyList); });

            _history.CountChanged
                .Throttle(TimeSpan.FromSeconds(1))
                .SelectMany(_ => Observable.Start(() => StateService.Set(nameof(_history), _history.Take(SettingsService.Current.HistoryCountLimit)), TaskPoolScheduler.Default))
                .Subscribe();

            SearchQuery = Environment.GetCommandLineArgs().Skip(1).FirstOrDefault();
        }



        public ReactiveCommand<Unit, IObservable<DirectoryEntryInfo>> Search { get; }
        public ReactiveCommand<Unit, Unit> Paste { get; }
        public ReactiveCommand<Unit, Unit> Open { get; }
        public ReactiveCommand<Unit, Unit> OpenSettings { get; }
        public IReactiveDerivedList<DirectoryEntryInfo> SearchResults => _searchResults.CreateDerivedCollection(directoryEntryInfo => directoryEntryInfo, orderer: (one, two) => one.Path.CompareTo(two.Path));
        public ReactiveList<string> History => _history;
        public bool IsNoResults => _isNoResults.Value;
        public string Domain => _adFacade.CurrentDomain;
        public SortDescription ListSortDescription { get => _listSortDescription; set => this.RaiseAndSetIfChanged(ref _listSortDescription, value); }
        public string SearchQuery { get => _searchQuery; set => this.RaiseAndSetIfChanged(ref _searchQuery, value); }
        public DirectoryEntryInfo SelectedSearchResult { get => _selectedSearchResult; set => this.RaiseAndSetIfChanged(ref _selectedSearchResult, value); }



        private readonly ADFacade _adFacade = Locator.Current.GetService<ADFacade>();
        private readonly ReactiveList<DirectoryEntryInfo> _searchResults;
        private readonly ReactiveList<string> _history;
        private readonly ObservableAsPropertyHelper<bool> _isNoResults;
        private SortDescription _listSortDescription;
        private string _searchQuery;
        private DirectoryEntryInfo _selectedSearchResult;
    }
}
