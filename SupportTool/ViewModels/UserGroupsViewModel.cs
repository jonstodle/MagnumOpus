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
using System.Reactive.Disposables;
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
        private readonly ReactiveList<string> _allGroups = new ReactiveList<string>();
        private readonly ReactiveList<string> _directGroups = new ReactiveList<string>();
        private readonly ListCollectionView _allGroupsCollectionView;
        private readonly ObservableAsPropertyHelper<bool> _isLoadingGroups;
        private UserObject _user;
        private bool _isShowingDirectGroups;
        private bool _isShowingAllGroups;
        private string _selectedDirectGroup;
        private string _selectedAllGroup;
        private string _groupFilter;
        private bool _useFuzzy;



        public UserGroupsViewModel()
        {
            _allGroupsCollectionView = new ListCollectionView(_allGroups);
            _allGroupsCollectionView.SortDescriptions.Add(new SortDescription());
            _allGroupsCollectionView.Filter = TextFilter;

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
                    return User.Principal.GetAllGroups().SubscribeOn(RxApp.TaskpoolScheduler)
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

            _isLoadingGroups = _getAllGroups
                .IsExecuting
                .ToProperty(this, x => x.IsLoadingGroups);

            _findDirectGroup = ReactiveCommand.CreateFromTask(() => NavigationService.ShowWindow<Views.GroupWindow>(_selectedDirectGroup));

            _findAllGroup = ReactiveCommand.CreateFromTask(() => NavigationService.ShowWindow<Views.GroupWindow>(_selectedAllGroup));

            this.WhenActivated(disposables =>
            {
                this
                    .WhenAnyValue(x => x.GroupFilter, y => y.UseFuzzy)
                    .Subscribe(_ => AllGroupsCollectionView?.Refresh())
                    .DisposeWith(disposables);

                _getAllGroups
                    .ThrownExceptions
                    .Subscribe(async ex => await _errorMessages.Handle(new MessageInfo(ex.Message, "Couldn't get groups")))
                    .DisposeWith(disposables);

                Observable.Merge(
                this.WhenAnyValue(x => x.User).WhereNotNull(),
                _openEditMemberOf.Select(_ => User))
                .Throttle(TimeSpan.FromSeconds(1), RxApp.MainThreadScheduler)
                .Do(_ => _directGroups.Clear())
                .SelectMany(x => GetDirectGroups(x.Principal.SamAccountName).SubscribeOn(RxApp.TaskpoolScheduler))
                .Select(x => x.Properties.Get<string>("cn"))
                .ObserveOnDispatcher()
                .Subscribe(x => _directGroups.Add(x))
                .DisposeWith(disposables);

                this
                    .WhenAnyValue(x => x.IsShowingDirectGroups)
                    .Where(x => x)
                    .Subscribe(_ => IsShowingAllGroups = false)
                    .DisposeWith(disposables);

                this
                    .WhenAnyValue(x => x.IsShowingAllGroups)
                    .Where(x => x)
                    .Subscribe(_ => IsShowingDirectGroups = false)
                    .DisposeWith(disposables);

                Observable.Merge(
                    _openEditMemberOf.ThrownExceptions,
                    _saveAllGroups.ThrownExceptions,
                    _saveDirectGroups.ThrownExceptions,
                    _findDirectGroup.ThrownExceptions,
                    _findAllGroup.ThrownExceptions)
                    .Subscribe(async ex => await _errorMessages.Handle(new MessageInfo(ex.Message)))
                    .DisposeWith(disposables);
            });
        }



        public ReactiveCommand OpenEditMemberOf => _openEditMemberOf;

        public ReactiveCommand SaveDirectGroups => _saveDirectGroups;

        public ReactiveCommand SaveAllGroups => _saveAllGroups;

        public ReactiveCommand GetAllGroups => _getAllGroups;

        public ReactiveCommand FindDirectGroup => _findDirectGroup;

        public ReactiveCommand FindAllGroup => _findAllGroup;

        public ReactiveList<string> AllGroups => _allGroups;

        public IReactiveDerivedList<string> DirectGroups => _directGroups.CreateDerivedCollection(x => x, orderer: (one, two) => one.CompareTo(two));

        public ListCollectionView AllGroupsCollectionView => _allGroupsCollectionView;

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

        public string SelectedDirectGroup
        {
            get { return _selectedDirectGroup; }
            set { this.RaiseAndSetIfChanged(ref _selectedDirectGroup, value); }
        }

        public string SelectedAllGroup
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
