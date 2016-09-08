using ReactiveUI;
using SupportTool.Helpers;
using SupportTool.Models;
using SupportTool.Services.ActiveDirectoryServices;
using SupportTool.Services.DialogServices;
using SupportTool.Services.NavigationServices;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
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
		private readonly ReactiveList<string> _directMemberOfGroups;
		private readonly ReactiveList<string> _allMemberOfGroups;
		private readonly ListCollectionView _directMemberOfGroupsView;
		private readonly ListCollectionView _allMemberOfGroupsView;
		private GroupObject _group;
		private bool _isShowingDirectMemberOf;
		private bool _isShowingMemberOf;
		private bool _isShowingMembers;
		private object _selectedDirectMemberOfGroup;
		private string _filterString;
		private bool _useFuzzy;
		private object _selectedAllMemberOfGroup;



		public GroupGroupsViewModel()
		{
			_directMemberOfGroups = new ReactiveList<string>();
			_allMemberOfGroups = new ReactiveList<string>();
			_directMemberOfGroupsView = new ListCollectionView(_directMemberOfGroups)
			{
				SortDescriptions =
				{
					new SortDescription()
				}
			};
			_allMemberOfGroupsView = new ListCollectionView(_allMemberOfGroups)
			{
				Filter = TextFilter,
				SortDescriptions =
				{
					new SortDescription()
				}
			};
			this
				.WhenAnyValue(x => x.FilterString, y => y.UseFuzzy)
				.Subscribe(_ => _allMemberOfGroupsView?.Refresh());

			_openAddGroups = ReactiveCommand.CreateFromTask(() => NavigationService.Current.NavigateTo<Views.AddGroupsWindow>(_group.CN));

			_openRemoveGroups = ReactiveCommand.CreateFromTask(() => NavigationService.Current.NavigateTo<Views.RemoveGroupsWindow>(_group.CN));

			_findDirectMemberOfGroup = ReactiveCommand.Create(() => MessageBus.Current.SendMessage(_selectedDirectMemberOfGroup as string, "search"));

			_getAllMemberOfGroups = ReactiveCommand.CreateFromObservable(
				() =>
				{
					_allMemberOfGroups.Clear();
					return GetAllGroupsImpl(_group.Principal.DisplayName).SubscribeOn(RxApp.TaskpoolScheduler)
								.TakeUntil(this.WhenAnyValue(x => x.IsShowingMemberOf).Where(x => !x));
				},
				this.WhenAnyValue(x => x.IsShowingMemberOf));
			_getAllMemberOfGroups
				.ObserveOnDispatcher()
				.Subscribe(x => _allMemberOfGroups.Add(x));
			_getAllMemberOfGroups
				.ThrownExceptions
				.Subscribe(ex => DialogService.ShowError(ex.Message, "Couldn't get groups"));

			_findAllMemberOfGroup = ReactiveCommand.Create(() => MessageBus.Current.SendMessage(_selectedAllMemberOfGroup as string, "search"));

			_openAddUsers = ReactiveCommand.CreateFromTask(() => NavigationService.Current.NavigateTo<Views.AddUsersWindow>(_group.CN));

			//_openRemoveUsers = ReactiveCommand.CreateFromTask(() => NavigationService.Current.NavigateTo<Views.RemoveUsersWindow>(_group.CN));

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
				this.WhenAnyValue(x => x.Group).NotNull(),
				_openAddGroups.Select(_ => _group),
				_openRemoveGroups.Select(_ => _group))
				.Throttle(TimeSpan.FromSeconds(1), RxApp.MainThreadScheduler)
				.Do(_ => _directMemberOfGroups.Clear())
				.SelectMany(x => GetDirectGroups(x.CN).SubscribeOn(RxApp.TaskpoolScheduler))
				.ObserveOnDispatcher()
				.Subscribe(x => _directMemberOfGroups.Add(x));

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

		public ReactiveList<string> DirectMemberOfGroups => _directMemberOfGroups;

		public ReactiveList<string> AllMemberOfGroups => _allMemberOfGroups;

		public ListCollectionView DirectMemberOfGroupsView => _directMemberOfGroupsView;

		public ListCollectionView AllMemberOfGroupsView => _allMemberOfGroupsView;

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



		private IObservable<string> GetDirectGroups(string identity) => Observable.Create<string>(observer =>
		{
			var disposed = false;

			var usr = ActiveDirectoryService.Current.GetGroup(identity).Wait();

			foreach (string item in usr.MemberOf)
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
			.Select(x => x.Properties.Get<string>("cn"))
			.Distinct();

			return groups.TakeUntil(groups.Select(_ => Observable.Timer(TimeSpan.FromSeconds(10))).Switch());
		}

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
