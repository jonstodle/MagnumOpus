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
	public class EditMembersDialogViewModel : ViewModelBase, IDialog
	{
		private readonly ReactiveCommand<string, GroupObject> _setGroup;
		private readonly ReactiveCommand<Unit, DirectoryEntry> _getGroupMembers;
		private readonly ReactiveCommand<Unit, IObservable<DirectoryEntry>> _search;
		private readonly ReactiveCommand<Unit, Unit> _addToGroup;
		private readonly ReactiveCommand<Unit, Unit> _removeFromGroup;
		private readonly ReactiveCommand<Unit, IEnumerable<string>> _save;
		private readonly ReactiveCommand<Unit, Unit> _cancel;
		private readonly ReactiveList<DirectoryEntry> _searchResults;
		private readonly ReactiveList<DirectoryEntry> _groupMembers;
		private readonly ReactiveList<DirectoryEntry> _membersToAdd;
		private readonly ReactiveList<DirectoryEntry> _membersToRemove;
		private readonly ListCollectionView _searchResultsView;
		private readonly ListCollectionView _groupMembersView;
		private readonly ObservableAsPropertyHelper<GroupObject> _group;
		private string _searchQuery;
		private object _selectedSearchResult;
		private object _selectedGroupMember;
		private Action _close;



		public EditMembersDialogViewModel()
		{
			_searchResults = new ReactiveList<DirectoryEntry>();
			_groupMembers = new ReactiveList<DirectoryEntry>();
			_membersToAdd = new ReactiveList<DirectoryEntry>();
			_membersToRemove = new ReactiveList<DirectoryEntry>();
			_searchResultsView = new ListCollectionView(_searchResults) { SortDescriptions = { new SortDescription("Path", ListSortDirection.Ascending) } };
			_groupMembersView = new ListCollectionView(_groupMembers) { SortDescriptions = { new SortDescription("Path", ListSortDirection.Ascending) } };

			_setGroup = ReactiveCommand.CreateFromObservable<string, GroupObject>(identity => ActiveDirectoryService.Current.GetGroup(identity));
			_setGroup
				.ToProperty(this, x => x.Group, out _group);

			_getGroupMembers = ReactiveCommand.CreateFromObservable(() => GetGroupMembersImpl(_group.Value).SubscribeOn(RxApp.TaskpoolScheduler));
			_getGroupMembers
				.ObserveOnDispatcher()
				.Subscribe(x => _groupMembers.Add(x));

			_search = ReactiveCommand.Create(() => ActiveDirectoryService.Current.SearchDirectory(_searchQuery).Take(1000).SubscribeOn(RxApp.TaskpoolScheduler));
			_search
				.Do(_ => _searchResults.Clear())
				.Switch()
				.ObserveOnDispatcher()
				.Subscribe(x => _searchResults.Add(x));

			_addToGroup = ReactiveCommand.Create(
				() =>
				{
					var de = _selectedSearchResult as DirectoryEntry;
					if (_groupMembers.Contains(de) || _membersToAdd.Contains(de)) return;
					_membersToRemove.Remove(de);
					_groupMembers.Add(de);
					_membersToAdd.Add(de);
				},
				this.WhenAnyValue(x => x.SelectedSearchResult).IsNotNull());

			_removeFromGroup = ReactiveCommand.Create(
				() =>
				{
					var de = _selectedGroupMember as DirectoryEntry;
					_groupMembers.Remove(de);
					if (_membersToAdd.Contains(de)) _membersToAdd.Remove(de);
					else _membersToRemove.Add(de);
				},
				this.WhenAnyValue(x => x.SelectedGroupMember).IsNotNull());

			_save = ReactiveCommand.CreateFromTask(
				async () => await SaveImpl(_group.Value, _membersToAdd, _membersToRemove),
				Observable.CombineLatest(_membersToAdd.CountChanged.StartWith(0), _membersToRemove.CountChanged.StartWith(0), (x, y) => x > 0 || y > 0));
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
					_setGroup.ThrownExceptions,
					_getGroupMembers.ThrownExceptions,
					_search.ThrownExceptions,
					_addToGroup.ThrownExceptions,
					_removeFromGroup.ThrownExceptions,
					_save.ThrownExceptions,
					_cancel.ThrownExceptions)
				.Subscribe(async ex => await _errorMessages.Handle(new MessageInfo(ex.Message)));
		}



		public ReactiveCommand SetGroup => _setGroup;

		public ReactiveCommand GetGroupMembers => _getGroupMembers;

		public ReactiveCommand Search => _search;

		public ReactiveCommand AddToGroup => _addToGroup;

		public ReactiveCommand RemoveFromGroup => _removeFromGroup;

		public ReactiveCommand Save => _save;

		public ReactiveCommand Cancel => _cancel;

		public ReactiveList<DirectoryEntry> SearchResults => _searchResults;

		public ReactiveList<DirectoryEntry> GroupMembers => _groupMembers;

		public ReactiveList<DirectoryEntry> MembersToAdd => _membersToAdd;

		public ReactiveList<DirectoryEntry> MembersToRemove => _membersToRemove;

		public ListCollectionView SearchResultsView => _searchResultsView;

		public ListCollectionView GroupMembersView => _groupMembersView;

		public GroupObject Group => _group.Value;

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

		public object SelectedGroupMember
		{
			get { return _selectedGroupMember; }
			set { this.RaiseAndSetIfChanged(ref _selectedGroupMember, value); }
		}



		private IObservable<DirectoryEntry> GetGroupMembersImpl(GroupObject group) => Observable.Create<DirectoryEntry>(observer =>
		{
			var disposed = false;

			foreach (Principal item in group.Principal.Members)
			{
				if (disposed) break;
				observer.OnNext(item.GetUnderlyingObject() as DirectoryEntry);
			}

			observer.OnCompleted();
			return () => disposed = true;
		});

		private IObservable<IEnumerable<string>> SaveImpl(GroupObject group, IEnumerable<DirectoryEntry> membersToAdd, IEnumerable<DirectoryEntry> membersToRemove) => Observable.Start(() =>
		{
			var result = new List<string>();

			foreach (var memberDe in membersToAdd)
			{
				var member = ActiveDirectoryService.Current.GetPrincipal(memberDe.Properties.Get<string>("samaccountname")).Wait();

				try { group.Principal.Members.Add(member); }
				catch (Exception ex) { result.Add($"{member.SamAccountName} - {ex.Message}"); }
			}

			foreach (var memberDe in membersToRemove)
			{
				var member = ActiveDirectoryService.Current.GetPrincipal(memberDe.Properties.Get<string>("samaccountname")).Wait();

				try { group.Principal.Members.Remove(member); }
				catch (Exception ex) { result.Add($"{member.SamAccountName} - {ex.Message}"); }
			}

			group.Principal.Save();

			return result;
		});



		public Task Opening(Action close, object parameter)
		{
			_close = close;

			if (parameter is string)
			{
				Observable.Return(parameter as string)
					.InvokeCommand(_setGroup);
			}

			return Task.FromResult<object>(null);
		}
	}
}
