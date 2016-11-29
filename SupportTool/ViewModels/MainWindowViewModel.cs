using ReactiveUI;
using SupportTool.Models;
using SupportTool.Services.ActiveDirectoryServices;
using SupportTool.Services.NavigationServices;
using SupportTool.Services.SettingsServices;
using SupportTool.Services.StateServices;
using System;
using System.ComponentModel;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Data;

namespace SupportTool.ViewModels
{
	public partial class MainWindowViewModel : ViewModelBase
	{
		private readonly ReactiveCommand<Unit, IObservable<DirectoryEntryInfo>> _search;
		private readonly ReactiveCommand<Unit, Unit> _paste;
		private readonly ReactiveCommand<Unit, Unit> _open;
		private readonly ReactiveCommand<Unit, Unit> _openSettings;
		private readonly ReactiveCommand<Unit, bool> _toggleShowVersion;
		private readonly ReactiveList<DirectoryEntryInfo> _searchResults;
		private readonly ReactiveList<string> _history;
		private readonly ListCollectionView _searchResultsView;
		private readonly ObservableAsPropertyHelper<bool> _showVersion;
		private SortDescription _listSortDescription;
		private string _searchQuery;
		private object _selectedSearchResult;
		private string _version;



		public MainWindowViewModel()
		{
			_searchResults = new ReactiveList<DirectoryEntryInfo>();
			_history = new ReactiveList<string>();
			_searchResultsView = new ListCollectionView(_searchResults)
			{
				SortDescriptions = { new SortDescription("Path", ListSortDirection.Ascending) }
			};

			var version = Assembly.GetExecutingAssembly().GetName().Version;
			var assemblyTime = Assembly.GetExecutingAssembly().GetLinkerTime();
			_version = $"{version.Major}.{version.Minor}.{assemblyTime.Day.ToString("00")}{assemblyTime.Month.ToString("00")}{assemblyTime.Year.ToString().Substring(2,2)}.{assemblyTime.Hour.ToString("00")}{assemblyTime.Minute.ToString("00")}{assemblyTime.Second.ToString("00")}";

			_search = ReactiveCommand.CreateFromTask(async () =>
			{
				if (_searchQuery.IsIPAddress())
				{
					await NavigationService.ShowWindow<Views.IPAddressWindow>(_searchQuery);
					return Observable.Empty<DirectoryEntryInfo>();
				}
				else return ActiveDirectoryService.Current.SearchDirectory(_searchQuery).Take(1000).Select(x => new Models.DirectoryEntryInfo(x)).SubscribeOn(RxApp.TaskpoolScheduler);
			});
			_search
				.Do(_ => _searchResults.Clear())
				.Switch()
				.ObserveOnDispatcher()
				.Subscribe(x => _searchResults.Add(x));

			_paste = ReactiveCommand.Create(() => { SearchQuery = Clipboard.GetText().ToUpperInvariant(); });

			_open = ReactiveCommand.CreateFromTask(
				async () => 
				{
					var de = _selectedSearchResult as DirectoryEntryInfo;
					var cn = de.CN;
					var principal = await ActiveDirectoryService.Current.GetPrincipal(cn);

					if (principal is UserPrincipal) await NavigationService.ShowWindow<Views.UserWindow>(principal.SamAccountName);
					else if (principal is ComputerPrincipal) await NavigationService.ShowWindow<Views.ComputerWindow>(principal.SamAccountName);
					else if (principal is GroupPrincipal) await NavigationService.ShowWindow<Views.GroupWindow>(principal.SamAccountName);

					if(!_history.Contains(cn)) _history.Insert(0, cn);
				},
				this.WhenAnyValue(x => x.SelectedSearchResult).Select(x => x != null));

			_openSettings = ReactiveCommand.CreateFromTask(() => NavigationService.ShowDialog<Views.SettingsWindow>());

			_toggleShowVersion = ReactiveCommand.Create<Unit, bool>(_ => !_showVersion.Value);

			_showVersion = _toggleShowVersion
				.ToProperty(this, x => x.ShowVersion, false);

			this.WhenAnyValue(x => x.ListSortDescription)
				.Subscribe(x =>
				{
					using (_searchResultsView.DeferRefresh())
					{
						_searchResultsView.SortDescriptions.Clear();
						_searchResultsView.SortDescriptions.Add(_listSortDescription);
					}
				});

			Observable.Merge(
				_search.ThrownExceptions,
				_paste.ThrownExceptions,
				_open.ThrownExceptions,
				_openSettings.ThrownExceptions)
				.Subscribe(async ex => await _errorMessages.Handle(new MessageInfo(ex.Message)));

			StateService.Get(nameof(_history), Enumerable.Empty<string>())
				.SubscribeOn(TaskPoolScheduler.Default)
				.ObserveOnDispatcher()
				.Subscribe(x => { using (_history.SuppressChangeNotifications()) _history.AddRange(x); });

			_history.CountChanged
				.Throttle(TimeSpan.FromSeconds(1))
				.SelectMany(_ => Observable.Start(() => StateService.Set(nameof(_history), _history.Take(SettingsService.Current.HistoryCountLimit))))
				.Subscribe();
		}



		public ReactiveCommand Search => _search;

		public ReactiveCommand<Unit, Unit> Paste => _paste;

		public ReactiveCommand Open => _open;

		public ReactiveCommand OpenSettings => _openSettings;

		public ReactiveCommand ToggleShowVersion => _toggleShowVersion;

		public ReactiveList<DirectoryEntryInfo> SearchResults => _searchResults;

		public ReactiveList<string> History => _history;

		public ListCollectionView SearchResultsView => _searchResultsView;

		public bool ShowVersion => _showVersion.Value;

		public string Version => _version;

		public string Domain => ActiveDirectoryService.Current.CurrentDomain;

		public SortDescription ListSortDescription
		{
			get { return _listSortDescription; }
			set { this.RaiseAndSetIfChanged(ref _listSortDescription, value); }
		}

		public string SearchQuery
		{
			get { return _searchQuery; }
			set { this.RaiseAndSetIfChanged(ref _searchQuery, value); }
		}

		public object SelectedSearchResult
		{
			get { return _selectedSearchResult; }
			set { this.RaiseAndSetIfChanged(ref _selectedSearchResult, value); }
		}
	}
}
