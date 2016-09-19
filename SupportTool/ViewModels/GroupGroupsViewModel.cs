using ReactiveUI;
using SupportTool.Models;
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
using System.Windows.Data;

namespace SupportTool.ViewModels
{
	public class GroupGroupsViewModel : ReactiveObject
	{
		private readonly ReactiveCommand<Unit, Unit> _openAddGroups;
		private readonly ReactiveCommand<Unit, Unit> _openRemoveGroups;
		private readonly ReactiveCommand<Unit, Unit> _findDirectMemberOfGroup;
		private readonly ReactiveCommand<Unit, string> _getAllMemberOfGroups;
		private readonly ReactiveCommand<Unit, Unit> _findAllMemberOfGroup;
		private readonly ReactiveCommand<Unit, Unit> _openAddUsers;
		private readonly ReactiveCommand<Unit, Unit> _openRemoveUsers;
		private readonly ReactiveCommand<Unit, Unit> _findMemberUser;
		private readonly ReactiveList<string> _directMemberOfGroups;
		private readonly ReactiveList<string> _allMemberOfGroups;
		private readonly ReactiveList<string> _memberUsers;
		private readonly ListCollectionView _directMemberOfGroupsView;
		private readonly ListCollectionView _allMemberOfGroupsView;
		private readonly ListCollectionView _memberUsersView;
		private GroupObject _group;
		private bool _isShowingDirectMemberOf;
		private bool _isShowingMemberOf;
		private bool _isShowingMembers;
		private object _selectedDirectMemberOfGroup;
		private string _filterString;
		private bool _useFuzzy;
		private object _selectedAllMemberOfGroup;
		private object _selectedMemberUser;



		public GroupGroupsViewModel()
		{
			_directMemberOfGroups = new ReactiveList<string>();
			_allMemberOfGroups = new ReactiveList<string>();
			_memberUsers = new ReactiveList<string>();
			_directMemberOfGroupsView = new ListCollectionView(_directMemberOfGroups)
			{
				SortDescriptions = { new SortDescription() }
			};
			_allMemberOfGroupsView = new ListCollectionView(_allMemberOfGroups)
			{
				Filter = TextFilter,
				SortDescriptions = { new SortDescription() }
			};
			_memberUsersView = new ListCollectionView(_memberUsers)
			{
				SortDescriptions = { new SortDescription() }
			};
			this
				.WhenAnyValue(x => x.FilterString, y => y.UseFuzzy)
				.Subscribe(_ => _allMemberOfGroupsView?.Refresh());

			_openAddGroups = ReactiveCommand.CreateFromTask(() => NavigationService.ShowDialog<Views.AddGroupsWindow>(_group.CN));

			_openRemoveGroups = ReactiveCommand.CreateFromTask(() => NavigationService.ShowDialog<Views.RemoveGroupsWindow>(_group.CN));

			_findDirectMemberOfGroup = ReactiveCommand.CreateFromTask(() => NavigationService.ShowWindow<Views.GroupWindow>(_selectedDirectMemberOfGroup as string));

			_getAllMemberOfGroups = ReactiveCommand.CreateFromObservable(
				() =>
				{
					_allMemberOfGroups.Clear();
					return GetAllGroupsImpl(_group.CN).SubscribeOn(RxApp.TaskpoolScheduler)
								.TakeUntil(this.WhenAnyValue(x => x.IsShowingMemberOf).Where(x => !x));
				},
				this.WhenAnyValue(x => x.IsShowingMemberOf));
			_getAllMemberOfGroups
				.ObserveOnDispatcher()
				.Subscribe(x => _allMemberOfGroups.Add(x));
			_getAllMemberOfGroups
				.ThrownExceptions
				.Subscribe(ex => DialogService.ShowError(ex.Message, "Couldn't get groups"));

			_findAllMemberOfGroup = ReactiveCommand.CreateFromTask(() => NavigationService.ShowWindow<Views.GroupWindow>(_selectedAllMemberOfGroup as string));

			_openAddUsers = ReactiveCommand.CreateFromTask(() => NavigationService.ShowDialog<Views.AddUsersWindow>(_group.CN));

			_openRemoveUsers = ReactiveCommand.CreateFromTask(() => NavigationService.ShowDialog<Views.RemoveUsersWindow>(_group.CN));

			_findMemberUser = ReactiveCommand.CreateFromTask(() => NavigationService.ShowWindow<Views.GroupWindow>(_selectedMemberUser as string));

			this
				.WhenAnyValue(x => x.IsShowingMemberOf, y => y.IsShowingMembers, (x, y) => x || y)
				.Where(x => x)
				.Subscribe(_ => IsShowingDirectMemberOf = false);
			this
				.WhenAnyValue(x => x.IsShowingDirectMemberOf, y => y.IsShowingMembers, (x, y) => x || y)
				.Where(x => x)
				.Subscribe(_ => IsShowingMemberOf = false);
			this
				.WhenAnyValue(x => x.IsShowingDirectMemberOf, y => y.IsShowingMemberOf, (x, y) => x || y)
				.Where(x => x)
				.Subscribe(_ => IsShowingMembers = false);

			Observable.Merge(
				this.WhenAnyValue(x => x.Group).WhereNotNull(),
				_openAddGroups.Select(_ => _group),
				_openRemoveGroups.Select(_ => _group))
				.Throttle(TimeSpan.FromSeconds(1), RxApp.MainThreadScheduler)
				.Do(_ => _directMemberOfGroups.Clear())
				.SelectMany(x => GetDirectGroups(x.CN).SubscribeOn(RxApp.TaskpoolScheduler))
				.ObserveOnDispatcher()
				.Subscribe(x => _directMemberOfGroups.Add(x));

			Observable.Merge(
				this.WhenAnyValue(x => x.Group).WhereNotNull(),
				_openAddUsers.Select(_ => _group),
				_openRemoveUsers.Select(_ => _group))
				.Throttle(TimeSpan.FromSeconds(1), RxApp.MainThreadScheduler)
				.Do(_ => _memberUsers.Clear())
				.SelectMany(x => GetMemberUsers(x.CN).SubscribeOn(RxApp.TaskpoolScheduler))
				.ObserveOnDispatcher()
				.Subscribe(x => _memberUsers.Add(x));

			this
				.WhenAnyValue(x => x.Group)
				.Subscribe(_ => ResetValues());
		}



		public ReactiveCommand OpenAddGroups => _openAddGroups;

		public ReactiveCommand OpenRemoveGroups => _openRemoveGroups;

		public ReactiveCommand FindDirectMemberOfGroup => _findDirectMemberOfGroup;

		public ReactiveCommand GetAllGroups => _getAllMemberOfGroups;

		public ReactiveCommand FindAllMemberOfGroup => _findAllMemberOfGroup;

		public ReactiveCommand OpenAddUsers => _openAddUsers;

		public ReactiveCommand OpenRemoveUsers => _openRemoveUsers;

		public ReactiveCommand FindMemberUser => _findMemberUser;

		public ReactiveList<string> DirectMemberOfGroups => _directMemberOfGroups;

		public ReactiveList<string> AllMemberOfGroups => _allMemberOfGroups;

		public ReactiveList<string> MemberUsers => _memberUsers;

		public ListCollectionView DirectMemberOfGroupsView => _directMemberOfGroupsView;

		public ListCollectionView AllMemberOfGroupsView => _allMemberOfGroupsView;

		public ListCollectionView MemberUsersView => _memberUsersView;

		public GroupObject Group
		{
			get { return _group; }
			set { this.RaiseAndSetIfChanged(ref _group, value); }
		}

		public bool IsShowingDirectMemberOf
		{
			get { return _isShowingDirectMemberOf; }
			set { this.RaiseAndSetIfChanged(ref _isShowingDirectMemberOf, value); }
		}

		public bool IsShowingMemberOf
		{
			get { return _isShowingMemberOf; }
			set { this.RaiseAndSetIfChanged(ref _isShowingMemberOf, value); }
		}

		public bool IsShowingMembers
		{
			get { return _isShowingMembers; }
			set { this.RaiseAndSetIfChanged(ref _isShowingMembers, value); }
		}

		public object SelectedDirectMemberOfGroup
		{
			get { return _selectedDirectMemberOfGroup; }
			set { this.RaiseAndSetIfChanged(ref _selectedDirectMemberOfGroup, value); }
		}

		public string FilterString
		{
			get { return _filterString; }
			set { this.RaiseAndSetIfChanged(ref _filterString, value); }
		}

		public bool UseFuzzy
		{
			get { return _useFuzzy; }
			set { this.RaiseAndSetIfChanged(ref _useFuzzy, value); }
		}

		public object SelectedAllMemberOfGroup
		{
			get { return _selectedAllMemberOfGroup; }
			set { this.RaiseAndSetIfChanged(ref _selectedAllMemberOfGroup, value); }
		}

		public object SelectedMemberUser
		{
			get { return _selectedMemberUser; }
			set { this.RaiseAndSetIfChanged(ref _selectedMemberUser, value); }
		}



		private IObservable<string> GetDirectGroups(string identity) => Observable.Create<string>(observer =>
		{
			var disposed = false;

			var group = ActiveDirectoryService.Current.GetGroup(identity).Wait();

			foreach (string item in group.MemberOf)
			{
				var de = ActiveDirectoryService.Current.GetGroups("distinguishedname", item).Take(1).Wait();

				if (disposed) break;
				observer.OnNext(de.Properties.Get<string>("cn"));
			}

			observer.OnCompleted();
			return () => disposed = true;
		});

		private IObservable<string> GetAllGroupsImpl(string identity)
		{
			var groups = ActiveDirectoryService.Current.GetGroup(identity).Wait().MemberOf.ToEnumerable<string>()
			.ToObservable()
			.SelectMany(x => ActiveDirectoryService.Current.GetParents(x))
			.Select(x =>
			{
				var cn = x.Properties.Get<string>("cn");
				x.Dispose();
				return cn;
			})
			.Distinct();

			return groups.TakeUntil(groups.Select(_ => Observable.Timer(TimeSpan.FromSeconds(10))).Switch());
		}

		private IObservable<string> GetMemberUsers(string identity) => Observable.Create<string>(observer =>
		{
			var disposed = false;

			var group = ActiveDirectoryService.Current.GetGroup(identity).Wait();

			foreach (Principal item in group.Principal.Members)
			{
				if (disposed) break;
				observer.OnNext(item.DisplayName);
			}

			observer.OnCompleted();
			return () => disposed = true;
		});

		bool TextFilter(object item)
		{
			if (!_filterString.HasValue()) { return true; }

			var itm = ((string)item).ToLowerInvariant().Replace(" ", string.Empty);

			if (_useFuzzy)
			{
				var filterString = _filterString.Replace(" ", string.Empty).ToLowerInvariant();

				var idx = 0;

				foreach (var letter in itm)
				{
					if (letter == filterString[idx])
					{
						idx += 1;
						if (idx >= filterString.Length) { return true; }
					}
				}
			}
			else
			{
				if (itm.Contains(_filterString.ToLowerInvariant()))
				{
					return true;
				}
			}

			return false;
		}



		private void ResetValues()
		{
			DirectMemberOfGroups.Clear();
			AllMemberOfGroups.Clear();
			IsShowingDirectMemberOf = false;
			IsShowingMemberOf = false;
			IsShowingMembers = false;
			FilterString = "";
			UseFuzzy = false;
		}
	}
}
