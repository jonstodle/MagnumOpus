using ReactiveUI;
using SupportTool.Models;
using SupportTool.Services.ActiveDirectoryServices;
using SupportTool.Services.DialogServices;
using SupportTool.Services.NavigationServices;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.DirectoryServices;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace SupportTool.ViewModels
{
	public class AddUsersWindowViewModel : ReactiveObject, IDialog
	{
		private readonly ReactiveCommand<Unit, IObservable<DirectoryEntry>> _searchForUsers;
		private readonly ReactiveCommand<Unit, Unit> _addToUsersToAdd;
		private readonly ReactiveCommand<Unit, Unit> _removeFromUsersToAdd;
		private readonly ReactiveCommand<Unit, IEnumerable<string>> _save;
		private readonly ReactiveList<DirectoryEntry> _searchResults;
		private readonly ReactiveList<DirectoryEntry> _usersToAdd;
		private readonly ListCollectionView _searchResultsView;
		private readonly ListCollectionView _usersToAddView;
		private readonly ObservableAsPropertyHelper<string> _windowTitle;
		private GroupObject _group;
		private string _searchString;
		private object _selectedSearchResult;
		private object _selectedUserToAdd;
		private Action _close;



		public AddUsersWindowViewModel()
		{
			_searchResults = new ReactiveList<DirectoryEntry>();
			_usersToAdd = new ReactiveList<DirectoryEntry>();
			_searchResultsView = new ListCollectionView(_searchResults)
			{
				SortDescriptions = { new SortDescription("Path", ListSortDirection.Ascending) }
			};
			_usersToAddView = new ListCollectionView(_usersToAdd)
			{
				SortDescriptions = { new SortDescription("Path", ListSortDirection.Ascending) }
			};

			_searchForUsers = ReactiveCommand.Create(
				() => ActiveDirectoryService.Current.GetUsers($"|(samaccountname={_searchString}*)(displayname={_searchString}*)")
						.SubscribeOn(RxApp.TaskpoolScheduler),
				this.WhenAnyValue(x => x.SearchString, x => x.HasValue()));
			_searchForUsers
				.Do(_ => _searchResults.Clear())
				.Switch()
				.ObserveOnDispatcher()
				.Subscribe(x => _searchResults.Add(x));

			_addToUsersToAdd = ReactiveCommand.Create(
				() =>
				{
					if (!_usersToAdd.Contains(_selectedSearchResult as DirectoryEntry)) _usersToAdd.Add(_selectedSearchResult as DirectoryEntry);
				},
				this.WhenAnyValue(x => x.SelectedSearchResult).Select(x => x != null));

			_removeFromUsersToAdd = ReactiveCommand.Create(
				() => { _usersToAdd.Remove(_selectedUserToAdd as DirectoryEntry); },
				this.WhenAnyValue(x => x.SelectedUserToAdd).Select(x => x != null));

			_save = ReactiveCommand.CreateFromObservable(
				() => SaveImpl(_usersToAdd, _group),
				_usersToAdd.CountChanged.Select(x => x > 0));
			_save
				.Take(1)
				.Subscribe(x =>
				{
					if (x.Count() > 0)
					{
						var builder = new StringBuilder();
						builder.AppendLine("The follwoing user(s) are already present and will not be added:");
						foreach (var user in x) builder.AppendLine(user);
						DialogService.ShowInfo(builder.ToString(), "Some users were not added");
					}

					_close();
				});

			_windowTitle = this
				.WhenAnyValue(x => x.Group)
				.WhereNotNull()
				.Select(x => $"Add users to {x.CN}")
				.ToProperty(this, x => x.WindowTitle, "");

			Observable.Merge(
				_searchForUsers.ThrownExceptions,
				_save.ThrownExceptions)
				.Subscribe(ex => DialogService.ShowError(ex.Message));
		}



		public ReactiveCommand SearchForUsers => _searchForUsers;

		public ReactiveCommand AddToUsersToAdd => _addToUsersToAdd;

		public ReactiveCommand RemoveFromUsersToAdd => _removeFromUsersToAdd;

		public ReactiveCommand Save => _save;

		public ReactiveList<DirectoryEntry> SearchResults => _searchResults;

		public ReactiveList<DirectoryEntry> UsersToAdd => _usersToAdd;

		public ListCollectionView SearchResultsView => _searchResultsView;

		public ListCollectionView UsersToAddView => _usersToAddView;

		public string WindowTitle => _windowTitle.Value;

		public GroupObject Group
		{
			get { return _group; }
			set { this.RaiseAndSetIfChanged(ref _group, value); }
		}

		public string SearchString
		{
			get { return _searchString; }
			set { this.RaiseAndSetIfChanged(ref _searchString, value); }
		}

		public object SelectedSearchResult
		{
			get { return _selectedSearchResult; }
			set { this.RaiseAndSetIfChanged(ref _selectedSearchResult, value); }
		}

		public object SelectedUserToAdd
		{
			get { return _selectedUserToAdd; }
			set { this.RaiseAndSetIfChanged(ref _selectedUserToAdd, value); }
		}



		private IObservable<IEnumerable<string>> SaveImpl(IEnumerable<DirectoryEntry> users, GroupObject group) => Observable.Start(() =>
		{
			var groupsNotAdded = new List<string>();
			foreach (var userDe in users)
			{
				var user = ActiveDirectoryService.Current.GetUser(userDe.Properties.Get<string>("samaccountname")).Wait();

				try { group.Principal.Members.Add(user.Principal); }
				catch { groupsNotAdded.Add(user.Principal.SamAccountName); }
			}
			group.Principal.Save();

			return groupsNotAdded;
		});

		private void ResetValues()
		{
			_searchResults.Clear();
			_usersToAdd.Clear();
			SearchString = "";
		}



		public async Task Opening(Action close, object parameter)
		{
			_close = close;

			ResetValues();

			if (parameter is string)
			{
				Group = await ActiveDirectoryService.Current.GetGroup(parameter as string);
			}
		}
	}
}
