using Microsoft.Win32;
using ReactiveUI;
using SupportTool.Models;
using SupportTool.Services.ActiveDirectoryServices;
using SupportTool.Services.ExportServices;
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
	public class UserGroupsViewModel : ViewModelBase
	{
		private readonly ReactiveCommand<Unit, Unit> _openEditMemberOf;
		private readonly ReactiveCommand<Unit, Unit> _saveDirectGroups;
		private readonly ReactiveCommand<Unit, Unit> _saveAllGroups;
		private readonly ReactiveCommand<Unit, DirectoryEntry> _getAllGroups;
		private readonly ReactiveCommand<Unit, Unit> _findDirectGroup;
		private readonly ReactiveCommand<Unit, Unit> _findAllGroup;
		private readonly ReactiveList<string> _allGroups;
        private readonly ReactiveList<string> _directGroups;
        private readonly ListCollectionView _allGroupsCollectionView;
        private readonly ListCollectionView _directGroupsCollectionView;
        private readonly ObservableAsPropertyHelper<bool> _isLoadingGroups;
        private UserObject _user;
        private bool _isShowingDirectGroups;
        private bool _isShowingAllGroups;
        private object _selectedDirectGroup;
		private object _selectedAllGroup;
        private string _groupFilter;
        private bool _useFuzzy;



        public UserGroupsViewModel()
		{
			_allGroups = new ReactiveList<string>();
            _directGroups = new ReactiveList<string>();

            _allGroupsCollectionView = new ListCollectionView(_allGroups);
            _allGroupsCollectionView.SortDescriptions.Add(new SortDescription());
            _allGroupsCollectionView.Filter = TextFilter;
            this
                .WhenAnyValue(x => x.GroupFilter, y => y.UseFuzzy)
                .Subscribe(_ => AllGroupsCollectionView?.Refresh());

            _directGroupsCollectionView = new ListCollectionView(_directGroups);
            _directGroupsCollectionView.SortDescriptions.Add(new SortDescription());

			_openEditMemberOf = ReactiveCommand.CreateFromTask(async () => await _dialogRequests.Handle(new DialogInfo(new Controls.EditMemberOfDialog(), _user.Principal.SamAccountName)));

			_saveDirectGroups = ReactiveCommand.CreateFromTask(async () =>
			{
				var saveFileDialog = new SaveFileDialog { Filter = ExcelService.ExcelFileFilter };
				if (saveFileDialog.ShowDialog() == true)
				{
					await ExcelService.SaveGroupsToExcelFile(_directGroups, saveFileDialog.FileName);
				}
			});

			_saveAllGroups = ReactiveCommand.CreateFromTask(async () =>
			{
				var saveFileDialog = new SaveFileDialog { Filter = ExcelService.ExcelFileFilter };
				if (saveFileDialog.ShowDialog() == true)
				{
					await ExcelService.SaveGroupsToExcelFile(_allGroups, saveFileDialog.FileName);
				}
			});

			_getAllGroups = ReactiveCommand.CreateFromObservable(
                () =>
                {
                    AllGroups.Clear();
                    return GetAllGroupsImpl(User.Principal.SamAccountName).SubscribeOn(RxApp.TaskpoolScheduler)
                            .TakeUntil(this.WhenAnyValue(x => x.IsShowingAllGroups).Where(x => !x));
                },
                this.WhenAnyValue(x => x.IsShowingAllGroups));
            _getAllGroups
                .ObserveOnDispatcher()
                .Select(x =>
				{
					var cn = x.Properties.Get<string>("cn");
					x.Dispose();
					return cn;
				})
                .Subscribe(x => AllGroups.Add(x));
            _getAllGroups
                .ThrownExceptions
                .Subscribe(async ex => await _errorMessages.Handle(new MessageInfo(ex.Message, "Couldn't get groups")));
            _getAllGroups
                .IsExecuting
                .ToProperty(this, x => x.IsLoadingGroups, out _isLoadingGroups);

			_findDirectGroup = ReactiveCommand.CreateFromTask(() => NavigationService.ShowWindow<Views.GroupWindow>(_selectedDirectGroup as string));

			_findAllGroup = ReactiveCommand.CreateFromTask(() => NavigationService.ShowWindow<Views.GroupWindow>(_selectedAllGroup as string));

            Observable.Merge(
                this.WhenAnyValue(x => x.User).WhereNotNull(),
                _openEditMemberOf.Select(_ => User))
				.Throttle(TimeSpan.FromSeconds(1), RxApp.MainThreadScheduler)
				.Do(_ => DirectGroups.Clear())
                .SelectMany(x => GetDirectGroups(x.Principal.SamAccountName).SubscribeOn(RxApp.TaskpoolScheduler))
				.Select(x => x.Properties.Get<string>("cn"))
                .ObserveOnDispatcher()
                .Subscribe(x => DirectGroups.Add(x));

			Observable.Merge(
				_saveAllGroups.ThrownExceptions,
				_saveDirectGroups.ThrownExceptions)
				.Subscribe(async ex => await _errorMessages.Handle(new MessageInfo(ex.Message)));

            this
                .WhenAnyValue(x => x.IsShowingDirectGroups)
                .Where(x => x)
                .Subscribe(_ => IsShowingAllGroups = false);

            this
                .WhenAnyValue(x => x.IsShowingAllGroups)
                .Where(x => x)
                .Subscribe(_ => IsShowingDirectGroups = false);
        }



		public ReactiveCommand OpenEditMemberOf => _openEditMemberOf;

		public ReactiveCommand SaveDirectGroups => _saveDirectGroups;

		public ReactiveCommand SaveAllGroups => _saveAllGroups;

		public ReactiveCommand GetAllGroups => _getAllGroups;

		public ReactiveCommand FindDirectGroup => _findDirectGroup;

		public ReactiveCommand FindAllGroup => _findAllGroup;

		public ReactiveList<string> AllGroups => _allGroups;

        public ReactiveList<string> DirectGroups => _directGroups;

        public ListCollectionView AllGroupsCollectionView => _allGroupsCollectionView;

        public ListCollectionView DirectGroupsCollectionView => _directGroupsCollectionView;

        public bool IsLoadingGroups => _isLoadingGroups.Value;

        public UserObject User
        {
            get { return _user; }
            set { this.RaiseAndSetIfChanged(ref _user, value); }
        }

        public bool IsShowingDirectGroups
        {
            get { return _isShowingDirectGroups; }
            set { this.RaiseAndSetIfChanged(ref _isShowingDirectGroups, value); }
        }

        public bool IsShowingAllGroups
        {
            get { return _isShowingAllGroups; }
            set { this.RaiseAndSetIfChanged(ref _isShowingAllGroups, value); }
        }

        public object SelectedDirectGroup
        {
            get { return _selectedDirectGroup; }
            set { this.RaiseAndSetIfChanged(ref _selectedDirectGroup, value); }
        }

		public object SelectedAllGroup
		{
			get { return _selectedAllGroup; }
			set { this.RaiseAndSetIfChanged(ref _selectedAllGroup, value); }
		}

		public string GroupFilter
        {
            get { return _groupFilter; }
            set { this.RaiseAndSetIfChanged(ref _groupFilter, value); }
        }

        public bool UseFuzzy
        {
            get { return _useFuzzy; }
            set { this.RaiseAndSetIfChanged(ref _useFuzzy, value); }
        }



		private IObservable<DirectoryEntry> GetDirectGroups(string identity) => Observable.Return(ActiveDirectoryService.Current.GetUser(identity).Wait())
			.SelectMany(x => x.Principal.GetGroups().ToObservable())
			.Select(x => x.GetUnderlyingObject() as DirectoryEntry);

        private IObservable<DirectoryEntry> GetAllGroupsImpl(string samAccountName)
        {
            var groups = ActiveDirectoryService.Current.GetUser(samAccountName).Wait().Principal.GetGroups()
            .ToObservable()
            .SelectMany(x => ActiveDirectoryService.Current.GetParents(x.Name))
            .Distinct(x => x.Path)
			.Publish()
			.RefCount();

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
