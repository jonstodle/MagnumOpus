using ReactiveUI;
using SupportTool.Models;
using SupportTool.Services.ActiveDirectoryServices;
using SupportTool.Services.DialogServices;
using SupportTool.Services.NavigationServices;
using System;
using System.ComponentModel;
using System.DirectoryServices;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Data;

namespace SupportTool.ViewModels
{
	public class UserGroupsViewModel : ReactiveObject
    {
		private readonly ReactiveCommand<Unit, Unit> openEditMemberOf;
        private readonly ReactiveCommand<Unit, DirectoryEntry> getAllGroups;
		private readonly ReactiveCommand<Unit, Unit> _findDirectGroup;
		private readonly ReactiveCommand<Unit, Unit> _findAllGroup;
		private readonly ReactiveList<string> allGroups;
        private readonly ReactiveList<string> directGroups;
        private readonly ListCollectionView allGroupsCollectionView;
        private readonly ListCollectionView directGroupsCollectionView;
        private readonly ObservableAsPropertyHelper<bool> isLoadingGroups;
        private UserObject user;
        private bool isShowingDirectGroups;
        private bool isShowingAllGroups;
        private object selectedDirectGroup;
		private object _selectedAllGroup;
        private string groupFilter;
        private bool useFuzzy;



        public UserGroupsViewModel()
        {
            allGroups = new ReactiveList<string>();
            directGroups = new ReactiveList<string>();

            allGroupsCollectionView = new ListCollectionView(allGroups);
            allGroupsCollectionView.SortDescriptions.Add(new SortDescription());
            allGroupsCollectionView.Filter = TextFilter;
            this
                .WhenAnyValue(x => x.GroupFilter, y => y.UseFuzzy)
                .Subscribe(_ => AllGroupsCollectionView?.Refresh());

            directGroupsCollectionView = new ListCollectionView(directGroups);
            directGroupsCollectionView.SortDescriptions.Add(new SortDescription());

			openEditMemberOf = ReactiveCommand.CreateFromTask(() => NavigationService.ShowDialog<Views.EditMemberOfWindow>(user.Principal.SamAccountName));

			getAllGroups = ReactiveCommand.CreateFromObservable(
                () =>
                {
                    AllGroups.Clear();
                    return GetAllGroupsImpl(User.Principal.SamAccountName).SubscribeOn(RxApp.TaskpoolScheduler)
                            .TakeUntil(this.WhenAnyValue(x => x.IsShowingAllGroups).Where(x => !x));
                },
                this.WhenAnyValue(x => x.IsShowingAllGroups));
            getAllGroups
                .ObserveOnDispatcher()
                .Select(x =>
				{
					var cn = x.Properties.Get<string>("cn");
					x.Dispose();
					return cn;
				})
                .Subscribe(x => AllGroups.Add(x));
            getAllGroups
                .ThrownExceptions
                .Subscribe(ex => DialogService.ShowError(ex.Message, "Couldn't get groups"));
            getAllGroups
                .IsExecuting
                .ToProperty(this, x => x.IsLoadingGroups, out isLoadingGroups);

			_findDirectGroup = ReactiveCommand.CreateFromTask(() => NavigationService.ShowWindow<Views.GroupWindow>(selectedDirectGroup as string));

			_findAllGroup = ReactiveCommand.CreateFromTask(() => NavigationService.ShowWindow<Views.GroupWindow>(_selectedAllGroup as string));

            Observable.Merge(
                this.WhenAnyValue(x => x.User).WhereNotNull(),
                openEditMemberOf.Select(_ => User))
				.Throttle(TimeSpan.FromSeconds(1), RxApp.MainThreadScheduler)
				.Do(_ => DirectGroups.Clear())
                .SelectMany(x => GetDirectGroups(x.Principal.SamAccountName).SubscribeOn(RxApp.TaskpoolScheduler))
                .ObserveOnDispatcher()
                .Subscribe(x => DirectGroups.Add(x.Properties.Get<string>("cn")));

            this
                .WhenAnyValue(x => x.IsShowingDirectGroups)
                .Where(x => x)
                .Subscribe(_ => IsShowingAllGroups = false);

            this
                .WhenAnyValue(x => x.IsShowingAllGroups)
                .Where(x => x)
                .Subscribe(_ => IsShowingDirectGroups = false);
        }



		public ReactiveCommand OpenEditMemberOf => openEditMemberOf;

        public ReactiveCommand GetAllGroups => getAllGroups;

		public ReactiveCommand FindDirectGroup => _findDirectGroup;

		public ReactiveCommand FindAllGroup => _findAllGroup;

		public ReactiveList<string> AllGroups => allGroups;

        public ReactiveList<string> DirectGroups => directGroups;

        public ListCollectionView AllGroupsCollectionView => allGroupsCollectionView;

        public ListCollectionView DirectGroupsCollectionView => directGroupsCollectionView;

        public bool IsLoadingGroups => isLoadingGroups.Value;

        public UserObject User
        {
            get { return user; }
            set { this.RaiseAndSetIfChanged(ref user, value); }
        }

        public bool IsShowingDirectGroups
        {
            get { return isShowingDirectGroups; }
            set { this.RaiseAndSetIfChanged(ref isShowingDirectGroups, value); }
        }

        public bool IsShowingAllGroups
        {
            get { return isShowingAllGroups; }
            set { this.RaiseAndSetIfChanged(ref isShowingAllGroups, value); }
        }

        public object SelectedDirectGroup
        {
            get { return selectedDirectGroup; }
            set { this.RaiseAndSetIfChanged(ref selectedDirectGroup, value); }
        }

		public object SelectedAllGroup
		{
			get { return _selectedAllGroup; }
			set { this.RaiseAndSetIfChanged(ref _selectedAllGroup, value); }
		}

		public string GroupFilter
        {
            get { return groupFilter; }
            set { this.RaiseAndSetIfChanged(ref groupFilter, value); }
        }

        public bool UseFuzzy
        {
            get { return useFuzzy; }
            set { this.RaiseAndSetIfChanged(ref useFuzzy, value); }
        }



        private IObservable<DirectoryEntry> GetDirectGroups(string identity) => Observable.Create<DirectoryEntry>(async observer =>
        {
            var disposed = false;

            var usr = await ActiveDirectoryService.Current.GetUser(identity);

            foreach (string item in usr.MemberOf)
            {
                var de = await ActiveDirectoryService.Current.GetGroups("distinguishedname", item).Take(1);

                if (disposed) break;
                observer.OnNext(de);
            }

            observer.OnCompleted();

            return () => disposed = true;
        });

        private IObservable<DirectoryEntry> GetAllGroupsImpl(string samAccountName)
        {
            var groups = ActiveDirectoryService.Current.GetUser(samAccountName).Wait().MemberOf.ToEnumerable<string>()
            .ToObservable()
            .SelectMany(x => ActiveDirectoryService.Current.GetParents(x))
            .Distinct(x => x.Path);

            return groups.TakeUntil(groups.Select(_ => Observable.Timer(TimeSpan.FromSeconds(10))).Switch());
        }

        bool TextFilter(object item)
        {
            if (!GroupFilter.HasValue()) { return true; }

            var itm = ((string)item).ToLowerInvariant().Replace(" ", string.Empty);

			if (UseFuzzy)
			{
				var filterString = GroupFilter.Replace(" ", string.Empty).ToLowerInvariant();

				var idx = 0;

				foreach (var letter in itm)
				{
					if (letter == filterString[idx])
					{
						idx += 1;
						if (idx >= filterString.Length) { return true; }
					}
				}

				return false;
			}
			else
			{
				return GroupFilter.Split(' ').All(x => itm.Contains(x.ToLowerInvariant()));
			}
        }
    }
}
