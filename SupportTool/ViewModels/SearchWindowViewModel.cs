using ReactiveUI;
using SupportTool.Services.ActiveDirectoryServices;
using SupportTool.Services.DialogServices;
using SupportTool.Services.NavigationServices;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace SupportTool.ViewModels
{
	public class SearchWindowViewModel : ReactiveObject, IDialog<string>
	{
		private readonly ReactiveCommand<Unit, IObservable<DirectoryEntry>> _search;
		private readonly ReactiveCommand<Unit, Unit> _choose;
		private readonly ReactiveList<DirectoryEntry> _searchResults;
		private readonly ListCollectionView _searchResultsView;
		private string _searchQuery;
		private object _selectedSearchResult;
		private Action<string> _close;



		public SearchWindowViewModel()
		{
			_searchResults = new ReactiveList<DirectoryEntry>();
			_searchResultsView = new ListCollectionView(_searchResults)
			{
				SortDescriptions = { new SortDescription("Path", ListSortDirection.Ascending) }
			};

			_search = ReactiveCommand.Create(() => ActiveDirectoryService.Current.SearchDirectory(_searchQuery).SubscribeOn(RxApp.TaskpoolScheduler));
			_search
				.Do(_ => _searchResults.Clear())
				.Switch()
				.ObserveOnDispatcher()
				.Subscribe(x => _searchResults.Add(x));

			_choose = ReactiveCommand.Create(() => _close(_selectedSearchResult as string));

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



		public Task Opening(Action<string> close, object parameter)
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
