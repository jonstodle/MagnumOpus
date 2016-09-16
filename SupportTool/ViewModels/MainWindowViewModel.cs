using Akavache;
using ReactiveUI;
using SupportTool.Models;
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
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace SupportTool.ViewModels
{
	public partial class MainWindowViewModel : ReactiveObject
	{
		private readonly ReactiveCommand<Unit, IObservable<DirectoryEntry>> _search;
		private readonly ReactiveCommand<Unit, Unit> _paste;
		private readonly ReactiveCommand<Unit, Unit> _open;
		private readonly ReactiveList<DirectoryEntry> _searchResults;
		private readonly ReactiveList<string> _history;
		private readonly ListCollectionView _searchResultsView;
		private string _searchQuery;
		private object _selectedSearchResult;



		public MainWindowViewModel()
		{
			_searchResults = new ReactiveList<DirectoryEntry>();
			_history = new ReactiveList<string>(BlobCache.LocalMachine.GetOrCreateObject(nameof(_history), () => Enumerable.Empty<string>()).Wait());
			_searchResultsView = new ListCollectionView(_searchResults)
			{
				SortDescriptions = { new SortDescription("Path", ListSortDirection.Ascending) }
			};

			_search = ReactiveCommand.CreateFromTask(async () =>
			{
				if (_searchQuery.IsIPAddress())
				{
					await NavigationService.ShowWindow<Views.IPAddressWindow>(_searchQuery);
					return Observable.Empty<DirectoryEntry>();
				}
				else return ActiveDirectoryService.Current.SearchDirectory(_searchQuery).Take(1000).SubscribeOn(RxApp.TaskpoolScheduler);
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
					var de = _selectedSearchResult as DirectoryEntry;
					var principal = await ActiveDirectoryService.Current.GetPrincipal(de.Properties.Get<string>("cn"));

					if (principal is UserPrincipal) await NavigationService.ShowWindow<Views.UserWindow>(principal.SamAccountName);
					else if (principal is ComputerPrincipal) await NavigationService.ShowWindow<Views.ComputerWindow>(principal.SamAccountName);
					else if (principal is GroupPrincipal) await NavigationService.ShowWindow<Views.GroupWindow>(principal.SamAccountName);

					_history.Insert(0, de.Properties.Get<string>("cn"));
				},
				this.WhenAnyValue(x => x.SelectedSearchResult).Select(x => x != null));

			Observable.Merge(
				_search.ThrownExceptions,
				_paste.ThrownExceptions,
				_open.ThrownExceptions)
				.Subscribe(ex => DialogService.ShowError(ex.Message));

			_history
				.CountChanged
				.Subscribe(_ => BlobCache.LocalMachine.InsertObject(nameof(_history), _history));
		}



		public ReactiveCommand Search => _search;

		public ReactiveCommand Paste => _paste;

		public ReactiveCommand Open => _open;

		public ReactiveList<DirectoryEntry> SearchResults => _searchResults;

		public ReactiveList<string> History => _history;

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
	}
}
