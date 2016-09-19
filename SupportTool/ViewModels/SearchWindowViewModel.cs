using ReactiveUI;
using SupportTool.Services.ActiveDirectoryServices;
using SupportTool.Services.DialogServices;
using SupportTool.Services.NavigationServices;
using System;
using System.ComponentModel;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Data;

namespace SupportTool.ViewModels
{
	public class SearchWindowViewModel : ReactiveObject, IDialog<Principal>
	{
		private readonly ReactiveCommand<Unit, IObservable<DirectoryEntry>> _search;
		private readonly ReactiveCommand<Unit, Unit> _choose;
		private readonly ReactiveList<DirectoryEntry> _searchResults;
		private readonly ListCollectionView _searchResultsView;
		private string _searchQuery;
		private object _selectedSearchResult;
		private Action<Principal> _close;



		public SearchWindowViewModel()
		{
			_searchResults = new ReactiveList<DirectoryEntry>();
			_searchResultsView = new ListCollectionView(_searchResults)
			{
				SortDescriptions = { new SortDescription("Path", ListSortDirection.Ascending) }
			};

			_search = ReactiveCommand.Create(() => ActiveDirectoryService.Current.SearchDirectory(_searchQuery).Take(1000).SubscribeOn(RxApp.TaskpoolScheduler));
			_search
				.Do(_ => _searchResults.Clear())
				.Switch()
				.ObserveOnDispatcher()
				.Subscribe(x => _searchResults.Add(x));

			_choose = ReactiveCommand.Create(
				() => _close(ActiveDirectoryService.Current.GetPrincipal((_selectedSearchResult as DirectoryEntry)?.Properties.Get<string>("cn")).Wait()),
				this.WhenAnyValue(x => x.SelectedSearchResult).Select(x => x != null));

			Observable.Merge(
				_search.ThrownExceptions,
				_choose.ThrownExceptions)
				.Subscribe(ex => DialogService.ShowError(ex.Message));
		}



		public ReactiveCommand Search => _search;

		public ReactiveCommand Choose => _choose;

		public ReactiveList<DirectoryEntry> SearchResults => _searchResults;

		public ListCollectionView SearchResultsView => _searchResultsView;

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



		private void ResetValues()
		{
			_searchResults.Clear();
			SearchQuery = "";
			SelectedSearchResult = null;
		}



		public Task Opening(Action<Principal> close, object parameter)
		{
			_close = close;

			ResetValues();

			if (parameter is string)
			{
				SearchQuery = parameter as string;
			}

			return Task.FromResult<object>(null);
		}
	}
}
