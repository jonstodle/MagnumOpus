using ReactiveUI;
using SupportTool.Models;
using SupportTool.Services.ActiveDirectoryServices;
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
	public class EditMemberOfDialogViewModel : ViewModelBase, IDialog
	{
		private readonly ReactiveCommand<string, Principal> _setPrincipal;
		private readonly ReactiveCommand<Unit, DirectoryEntry> _getPrincipalMembers;
		private readonly ReactiveCommand<Unit, IObservable<DirectoryEntry>> _search;
		private readonly ReactiveCommand<Unit, Unit> _addToPrincipal;
		private readonly ReactiveCommand<Unit, Unit> _removeFromPrincipal;
		private readonly ReactiveCommand<Unit, IEnumerable<string>> _save;
		private readonly ReactiveCommand<Unit, Unit> _cancel;
		private readonly ReactiveList<DirectoryEntry> _searchResults;
		private readonly ReactiveList<DirectoryEntry> _principalMembers;
		private readonly ReactiveList<DirectoryEntry> _membersToAdd;
		private readonly ReactiveList<DirectoryEntry> _membersToRemove;
		private readonly ListCollectionView _searchResultsView;
		private readonly ListCollectionView _principalMembersView;
		private readonly ObservableAsPropertyHelper<Principal> _principal;
		private string _searchQuery;
		private object _selectedSearchResult;
		private object _selectedPrincipalMember;
		private Action _close;



		public EditMemberOfDialogViewModel()
		{
			_searchResults = new ReactiveList<DirectoryEntry>();
			_principalMembers = new ReactiveList<DirectoryEntry>();
			_membersToAdd = new ReactiveList<DirectoryEntry>();
			_membersToRemove = new ReactiveList<DirectoryEntry>();
			_searchResultsView = new ListCollectionView(_searchResults) { SortDescriptions = { new SortDescription("Path", ListSortDirection.Ascending) } };
			_principalMembersView = new ListCollectionView(_principalMembers) { SortDescriptions = { new SortDescription("Path", ListSortDirection.Ascending) } };

			_setPrincipal = ReactiveCommand.CreateFromObservable<string, Principal>(identity => ActiveDirectoryService.Current.GetPrincipal(identity));
			_setPrincipal
				.ToProperty(this, x => x.Principal, out _principal);

			_getPrincipalMembers = ReactiveCommand.CreateFromObservable(() => GetPrincipalMembersImpl(_principal.Value).SubscribeOn(RxApp.TaskpoolScheduler));
			_getPrincipalMembers
				.ObserveOnDispatcher()
				.Subscribe(x => _principalMembers.Add(x));

			_search = ReactiveCommand.Create(() => ActiveDirectoryService.Current.SearchDirectory(_searchQuery).Take(1000).SubscribeOn(RxApp.TaskpoolScheduler));
			_search
				.Do(_ => _searchResults.Clear())
				.Switch()
				.ObserveOnDispatcher()
				.Subscribe(x => _searchResults.Add(x));

			_addToPrincipal = ReactiveCommand.Create(
				() =>
				{
					var de = _selectedSearchResult as DirectoryEntry;
					if (_principalMembers.Contains(de) || _membersToAdd.Contains(de)) return;
					_membersToRemove.Remove(de);
					_principalMembers.Add(de);
					_membersToAdd.Add(de);
				},
				this.WhenAnyValue(x => x.SelectedSearchResult).IsNotNull());

			_removeFromPrincipal = ReactiveCommand.Create(
				() =>
				{
					var de = _selectedPrincipalMember as DirectoryEntry;
					_principalMembers.Remove(de);
					if (_membersToAdd.Contains(de)) _membersToAdd.Remove(de);
					else _membersToRemove.Add(de);
				},
				this.WhenAnyValue(x => x.SelectedPrincipalMember).IsNotNull());

			_save = ReactiveCommand.CreateFromTask(
				async () => await SaveImpl(_principal.Value, _membersToAdd, _membersToRemove),
				Observable.CombineLatest(_membersToAdd.CountChanged.StartWith(0), _membersToRemove.CountChanged.StartWith(0), (x,y)=> x > 0 || y > 0));
			_save
				.Subscribe(async x =>
				{
					if (x.Count() > 0)
					{
						var builder = new StringBuilder();
						foreach (var message in x) builder.AppendLine(message);
						await _infoMessages.Handle(new MessageInfo($"The following messages were generated:\n{builder.ToString()}"));
					}

					_close();
				});

			_cancel = ReactiveCommand.Create(() => _close());

			Observable.Merge(
					_search.ThrownExceptions,
					_addToPrincipal.ThrownExceptions,
					_removeFromPrincipal.ThrownExceptions,
					_save.ThrownExceptions)
				.Subscribe(async ex => await _errorMessages.Handle(new MessageInfo(ex.Message)));
		}



		public ReactiveCommand SetPrincipal => _setPrincipal;

		public ReactiveCommand GetPrincipalMembers => _getPrincipalMembers;

		public ReactiveCommand Search => _search;

		public ReactiveCommand AddToPrincipal => _addToPrincipal;

		public ReactiveCommand RemoveFromPrincipal => _removeFromPrincipal;

		public ReactiveCommand Save => _save;

		public ReactiveCommand Cancel => _cancel;

		public ReactiveList<DirectoryEntry> SearchResults => _searchResults;

		public ReactiveList<DirectoryEntry> PrincipalMembers => _principalMembers;

		public ReactiveList<DirectoryEntry> MembersToAdd => _membersToAdd;

		public ReactiveList<DirectoryEntry> MembersToRemove => _membersToRemove;

		public ListCollectionView SearchResultsView => _searchResultsView;

		public ListCollectionView PrincipalMembersView => _principalMembersView;


		public Principal Principal => _principal.Value;

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

		public object SelectedPrincipalMember
		{
			get { return _selectedPrincipalMember; }
			set { this.RaiseAndSetIfChanged(ref _selectedPrincipalMember, value); }
		}



		private IObservable<DirectoryEntry> GetPrincipalMembersImpl(Principal principal) => principal.GetGroups()
			.ToObservable()
			.Select(x => x.GetUnderlyingObject() as DirectoryEntry);

		private IObservable<IEnumerable<string>> SaveImpl(Principal principal, IEnumerable<DirectoryEntry> membersToAdd, IEnumerable<DirectoryEntry> membersToRemove) => Observable.Start(() =>
		{
			var result = new List<string>();

			foreach (var groupDe in membersToAdd)
			{
				var group = ActiveDirectoryService.Current.GetGroup(groupDe.Properties.Get<string>("cn")).Wait();

				try
				{
					group.Principal.Members.Add(principal);
					group.Principal.Save();
				}
				catch (Exception ex) { result.Add($"{group.CN} - {ex.Message}"); }
			}

			foreach (var groupDe in membersToRemove)
			{
				var group = ActiveDirectoryService.Current.GetGroup(groupDe.Properties.Get<string>("cn")).Wait();

				try
				{
					group.Principal.Members.Remove(principal);
					group.Principal.Save();
				}
				catch (Exception ex) { result.Add($"{group.CN} - {ex.Message}"); }
			}

			return result;
		});



		public Task Opening(Action close, object parameter)
		{
			_close = close;

			if (parameter is string)
			{
				Observable.Return(parameter as string)
					.InvokeCommand(_setPrincipal);
			}

			return Task.FromResult<object>(null);
		}
	}
}
