using ReactiveUI;
using SupportTool.Helpers;
using SupportTool.Models;
using SupportTool.Services.ActiveDirectoryServices;
using SupportTool.Services.DialogServices;
using SupportTool.Services.NavigationServices;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace SupportTool.ViewModels
{
	public class AddGroupsWindowViewModel : ReactiveObject, IDialog
	{
		private readonly ReactiveCommand<Unit, DirectoryEntry> searchForGroups;
		private readonly ReactiveCommand<Unit, Unit> addToGroupsToAdd;
		private readonly ReactiveCommand<Unit, bool> removeFromGroupsToAdd;
		private readonly ReactiveCommand<Unit, IEnumerable<string>> addPrincipalToGroups;
		private readonly ReactiveList<DirectoryEntry> searchResults;
		private readonly ReactiveList<DirectoryEntry> groupsToAdd;
		private readonly ListCollectionView searchResultsView;
		private readonly ListCollectionView groupsToAddView;
		private readonly ObservableAsPropertyHelper<string> windowTitle;
		private readonly ObservableAsPropertyHelper<bool> isSearchingForGroups;
		private Principal principal;
		private string searchQuery;
		private object selectedSearchResult;
		private object selectedGroupToAdd;
		private Action _close;



		public AddGroupsWindowViewModel()
		{
			searchResults = new ReactiveList<DirectoryEntry>();
			groupsToAdd = new ReactiveList<DirectoryEntry>();

			searchResultsView = new ListCollectionView(searchResults);
			searchResultsView.SortDescriptions.Add(new SortDescription("Path", ListSortDirection.Ascending));

			groupsToAddView = new ListCollectionView(groupsToAdd);
			groupsToAddView.SortDescriptions.Add(new SortDescription("Path", ListSortDirection.Ascending));

			searchForGroups = ReactiveCommand.CreateFromObservable(() =>
			{
				searchResults.Clear();
				return ActiveDirectoryService.Current.GetGroups($"cn", $"{searchQuery}*", "cn").SubscribeOn(RxApp.TaskpoolScheduler);
			});
			searchForGroups
				.Subscribe(x => searchResults.Add(x));
			searchForGroups
				.ThrownExceptions
				.Subscribe(ex => DialogService.ShowError(ex.Message));
			searchForGroups
				.IsExecuting
				.ToProperty(this, x => x.IsSearchingForGroups, out isSearchingForGroups);

			addToGroupsToAdd = ReactiveCommand.Create(
				() =>
				{
					var de = selectedSearchResult as DirectoryEntry;
					if (!groupsToAdd.Contains(de))
					{
						groupsToAdd.Add(de);
					}
				},
				this.WhenAnyValue(x => x.SelectedSearchResult).Select(x => x != null));
			addToGroupsToAdd
				.ThrownExceptions
				.Subscribe(ex => DialogService.ShowError(ex.Message, "Could not add group"));

			removeFromGroupsToAdd = ReactiveCommand.Create(
				() => groupsToAdd.Remove(SelectedSearchResult as DirectoryEntry),
				this.WhenAnyValue(x => x.SelectedGroupToAdd).Select(x => x != null));
			removeFromGroupsToAdd
				.ThrownExceptions
				.Subscribe(ex => DialogService.ShowError(ex.Message, "Could not remove group"));

			addPrincipalToGroups = ReactiveCommand.CreateFromObservable(
				() => AddPrincipalToGroupsImpl(groupsToAdd, principal),
				groupsToAdd.CountChanged.Select(x => x > 0));
			addPrincipalToGroups
				.Take(1)
				.Subscribe(x =>
				{
					if (x.Count() > 0)
					{
						var builder = new StringBuilder();
						builder.AppendLine("The follwoing group(s) are already present and will not be added:");
						foreach (var group in x) builder.AppendLine(group);
						DialogService.ShowInfo(builder.ToString(), "Some groups were not added");
					}

					_close();
				});
			addPrincipalToGroups
				.ThrownExceptions
				.Subscribe(ex => DialogService.ShowError(ex.Message, "Could not add groups"));

			this
				.WhenAnyValue(x => x.Principal)
				.NotNull()
				.Select(x => $"Add groups to {x.DisplayName}")
				.ToProperty(this, x => x.WindowTitle, out windowTitle);
		}



		public ReactiveCommand SearchForGroups => searchForGroups;

		public ReactiveCommand AddToGroupsToAdd => addToGroupsToAdd;

		public ReactiveCommand RemoveFromGroupsToAdd => removeFromGroupsToAdd;

		public ReactiveCommand AddPrincipalToGroups => addPrincipalToGroups;

		public ReactiveList<DirectoryEntry> SearchResults => searchResults;

		public ReactiveList<DirectoryEntry> GroupsToAdd => groupsToAdd;

		public ListCollectionView SearchResultsView => searchResultsView;

		public ListCollectionView GroupsToAddView => groupsToAddView;

		public string WindowTitle => windowTitle.Value;

		public bool IsSearchingForGroups => isSearchingForGroups.Value;

		public Principal Principal
		{
			get { return principal; }
			set { this.RaiseAndSetIfChanged(ref principal, value); }
		}

		public string SearchQuery
		{
			get { return searchQuery; }
			set { this.RaiseAndSetIfChanged(ref searchQuery, value); }
		}

		public object SelectedSearchResult
		{
			get { return selectedSearchResult; }
			set { this.RaiseAndSetIfChanged(ref selectedSearchResult, value); }
		}

		public object SelectedGroupToAdd
		{
			get { return selectedGroupToAdd; }
			set { this.RaiseAndSetIfChanged(ref selectedGroupToAdd, value); }
		}



		private void ResetValues()
		{
			SearchQuery = "";
			SearchResults.Clear();
			GroupsToAdd.Clear();
		}



		private IObservable<IEnumerable<string>> AddPrincipalToGroupsImpl(IEnumerable<DirectoryEntry> groups, Principal principal) => Observable.Start(() =>
		{
			var groupsNotAdded = new List<string>();
			foreach (var groupDe in groups)
			{
				var group = ActiveDirectoryService.Current.GetGroup(groupDe.Properties.Get<string>("cn")).Wait();

				try
				{
					group.Principal.Members.Add(principal);
					group.Principal.Save();
				}
				catch { groupsNotAdded.Add(group.CN); }
			}

			return groupsNotAdded;
		});



		public async Task Opening(Action close, object parameter)
		{
			_close = close;

			ResetValues();

			if (parameter is string)
			{
				var param = (string)parameter;

				Principal = await ActiveDirectoryService.Current.GetPrincipal(param).Take(1);
			}
		}
	}
}
