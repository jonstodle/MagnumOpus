using ReactiveUI;
using MagnumOpus.Models;
using MagnumOpus.Services.ActiveDirectoryServices;
using MagnumOpus.Services.NavigationServices;
using MagnumOpus.Services.SettingsServices;
using MagnumOpus.Services.StateServices;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reflection;
using System.Windows;

namespace MagnumOpus.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel()
        {
            _searchResults = new ReactiveList<DirectoryEntryInfo>();
            _history = new ReactiveList<string>();

            _search = ReactiveCommand.Create(() => _searchQuery.IsIPAddress()
                ? Observable.FromAsync(() => NavigationService.ShowWindow<Views.IPAddressWindow>(_searchQuery)).SelectMany(_ => Observable.Empty<DirectoryEntryInfo>())
                : ActiveDirectoryService.Current.SearchDirectory(_searchQuery.Trim()).Take(1000).Select(x => new Models.DirectoryEntryInfo(x)).SubscribeOn(RxApp.TaskpoolScheduler));
            _search
                .Do(_ => _searchResults.Clear())
                .Switch()
                .ObserveOnDispatcher()
                .Subscribe(x => _searchResults.Add(x));

            _paste = ReactiveCommand.Create(() => { SearchQuery = Clipboard.GetText().Trim().ToUpperInvariant(); });

            _open = ReactiveCommand.CreateFromTask(
                async () =>
                {
                    var principal = await ActiveDirectoryService.Current.GetPrincipal(_selectedSearchResult.CN);

                    await NavigationService.ShowPrincipalWindow(principal);

                    if (!_history.Contains(_selectedSearchResult.CN)) _history.Insert(0, _selectedSearchResult.CN);
                },
                this.WhenAnyValue(x => x.SelectedSearchResult).Select(x => x != null));

            _openSettings = ReactiveCommand.CreateFromTask(() => NavigationService.ShowDialog<Views.SettingsWindow>());

            _isNoResults = _search
                .Select(x => Observable.Concat(
                    Observable.Return(false),
                    x.Aggregate(0, (acc, curr) => acc + 1).Select(y => y == 0)))
                .Switch()
                .ObserveOnDispatcher()
                .ToProperty(this, x => x.IsNoResults);

            Observable.Merge(
                _search.ThrownExceptions.Select(ex => ("Could not complete search", ex.Message)),
                _paste.ThrownExceptions.Select(ex => ("Could not not paste text", ex.Message)),
                _open.ThrownExceptions.Select(ex => ("Could not open AD object", ex.Message)),
                _openSettings.ThrownExceptions.Select(ex => ("Could not open settings", ex.Message)))
                .SelectMany(x => _messages.Handle(new MessageInfo(MessageType.Error, x.Item2, x.Item1)))
                .Subscribe();

            StateService.Get(nameof(_history), Enumerable.Empty<string>())
                .SubscribeOn(TaskPoolScheduler.Default)
                .ObserveOnDispatcher()
                .Subscribe(x => { using (_history.SuppressChangeNotifications()) _history.AddRange(x); });

            _history.CountChanged
                .Throttle(TimeSpan.FromSeconds(1))
                .SelectMany(_ => Observable.Start(() => StateService.Set(nameof(_history), _history.Take(SettingsService.Current.HistoryCountLimit))))
                .Subscribe();

            SearchQuery = Environment.GetCommandLineArgs().Skip(1).FirstOrDefault();
        }



        public ReactiveCommand Search => _search;

        public ReactiveCommand<Unit, Unit> Paste => _paste;

        public ReactiveCommand Open => _open;

        public ReactiveCommand OpenSettings => _openSettings;

        public IReactiveDerivedList<DirectoryEntryInfo> SearchResults => _searchResults.CreateDerivedCollection(x => x, orderer: (one, two) => one.Path.CompareTo(two.Path));

        public ReactiveList<string> History => _history;

        public bool IsNoResults => _isNoResults.Value;

        public string Domain => ActiveDirectoryService.Current.CurrentDomain;

        public SortDescription ListSortDescription { get => _listSortDescription; set => this.RaiseAndSetIfChanged(ref _listSortDescription, value); }

        public string SearchQuery { get => _searchQuery; set => this.RaiseAndSetIfChanged(ref _searchQuery, value); }

        public DirectoryEntryInfo SelectedSearchResult { get => _selectedSearchResult; set => this.RaiseAndSetIfChanged(ref _selectedSearchResult, value); }



        private readonly ReactiveCommand<Unit, IObservable<DirectoryEntryInfo>> _search;
        private readonly ReactiveCommand<Unit, Unit> _paste;
        private readonly ReactiveCommand<Unit, Unit> _open;
        private readonly ReactiveCommand<Unit, Unit> _openSettings;
        private readonly ReactiveList<DirectoryEntryInfo> _searchResults;
        private readonly ReactiveList<string> _history;
        private readonly ObservableAsPropertyHelper<bool> _isNoResults;
        private SortDescription _listSortDescription;
        private string _searchQuery;
        private DirectoryEntryInfo _selectedSearchResult;
    }
}
