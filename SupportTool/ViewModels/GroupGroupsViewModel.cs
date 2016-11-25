using Microsoft.Win32;
using ReactiveUI;
using SupportTool.Models;
using SupportTool.Services.ActiveDirectoryServices;
using SupportTool.Services.ExportServices;
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
	public class GroupGroupsViewModel : ViewModelBase
	{
		private readonly ReactiveCommand<Unit, Unit> _openEditMemberOf;
		private readonly ReactiveCommand<Unit, Unit> _saveDirectGroups;
		private readonly ReactiveCommand<Unit, Unit> _findDirectMemberOfGroup;
		private readonly ReactiveCommand<Unit, string> _getAllMemberOfGroups;
		private readonly ReactiveCommand<Unit, Unit> _findAllMemberOfGroup;
		private readonly ReactiveCommand<Unit, Unit> _saveAllGroups;
		private readonly ReactiveCommand<Unit, Unit> _openEditMembers;
		private readonly ReactiveCommand<Unit, Unit> _saveMembers;
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

			_openEditMemberOf = ReactiveCommand.CreateFromTask(async () => await _dialogRequests.Handle(new DialogInfo(new Controls.EditMemberOfDialog(), _group.CN)));

			_saveDirectGroups = ReactiveCommand.CreateFromTask(async () =>
			{
				var saveFileDialog = new SaveFileDialog { Filter = ExcelService.ExcelFileFilter };
				if (saveFileDialog.ShowDialog() == true)
				{
					await ExcelService.SaveGroupsToExcelFile(_directMemberOfGroups, saveFileDialog.FileName);
				}
			});

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
				.Subscribe(async ex => await _errorMessages.Handle(new MessageInfo(ex.Message, "Couldn't get groups")));

			_findAllMemberOfGroup = ReactiveCommand.CreateFromTask(() => NavigationService.ShowWindow<Views.GroupWindow>(_selectedAllMemberOfGroup as string));

			_saveAllGroups = ReactiveCommand.CreateFromTask(async () =>
			{
				var saveFileDialog = new SaveFileDialog { Filter = ExcelService.ExcelFileFilter };
				if (saveFileDialog.ShowDialog() == true)
				{
					await ExcelService.SaveGroupsToExcelFile(_allMemberOfGroups, saveFileDialog.FileName);
				}
			});

			_openEditMembers = ReactiveCommand.CreateFromTask(async () => await _dialogRequests.Handle(new DialogInfo(new Controls.EditMembersDialog(), _group.CN)));

			_saveMembers = ReactiveCommand.CreateFromTask(async () =>
			{
				var saveFileDialog = new SaveFileDialog { Filter = ExcelService.ExcelFileFilter };
				if (saveFileDialog.ShowDialog() == true)
				{
					await ExcelService.SaveUsersToExcelFile(_memberUsers, saveFileDialog.FileName);
				}
			});

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
				_openEditMemberOf.Select(_ => _group))
				.Throttle(TimeSpan.FromSeconds(1), RxApp.MainThreadScheduler)
				.Do(_ => _directMemberOfGroups.Clear())
				.SelectMany(x => GetDirectGroups(x.CN).SubscribeOn(RxApp.TaskpoolScheduler))
				.ObserveOnDispatcher()
				.Subscribe(x => _directMemberOfGroups.Add(x));

			Observable.Merge(
				this.WhenAnyValue(x => x.Group).WhereNotNull(),
				_openEditMembers.Select(_ => _group))
				.Throttle(TimeSpan.FromSeconds(1), RxApp.MainThreadScheduler)
				.Do(_ => _memberUsers.Clear())
				.SelectMany(x => GetMemberUsers(x.CN).SubscribeOn(RxApp.TaskpoolScheduler))
				.ObserveOnDispatcher()
				.Subscribe(x => _memberUsers.Add(x));
		}



		public ReactiveCommand OpenEditMemberOf => _openEditMemberOf;

		public ReactiveCommand SaveDirectGroups => _saveDirectGroups;

		public ReactiveCommand FindDirectMemberOfGroup => _findDirectMemberOfGroup;

		public ReactiveCommand GetAllGroups => _getAllMemberOfGroups;

		public ReactiveCommand FindAllMemberOfGroup => _findAllMemberOfGroup;

		public ReactiveCommand SaveAllGroups => _saveAllGroups;

		public ReactiveCommand OpenEditMembers => _openEditMembers;

		public ReactiveCommand SaveMembers => _saveMembers;

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



		private IObservable<string> GetDirectGroups(string identity) => Observable.Return(ActiveDirectoryService.Current.GetGroup(identity).Wait())
			.SelectMany(x => x.Principal.GetGroups().ToObservable())
			.Select(x => x.Name);

		private IObservable<string> GetAllGroupsImpl(string identity)
		{
			var groups = ActiveDirectoryService.Current.GetParents(ActiveDirectoryService.Current.GetGroup(identity).Wait().Principal.GetGroups().Select(x => x.Name))
			.Select(x =>
			{
				var name = x.Properties.Get<string>("name");
				x.Dispose();
				return name;
			})
			.Distinct()
			.Publish()
			.RefCount();

			return groups.TakeUntil(groups.Select(_ => Observable.Timer(TimeSpan.FromSeconds(10))).Switch());
		}

		private IObservable<string> GetMemberUsers(string identity) => Observable.Create<string>(observer =>
		{
			var disposed = false;

			var group = ActiveDirectoryService.Current.GetGroup(identity).Wait();

			foreach (Principal item in group.Principal.Members)
			{
				if (disposed) break;
				observer.OnNext(item.Name);
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
	}
}
