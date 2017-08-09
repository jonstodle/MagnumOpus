using Microsoft.Win32;
using ReactiveUI;
using System;
using System.ComponentModel;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Data;
using System.Reactive.Concurrency;
using DocumentFormat.OpenXml.Spreadsheet;
using MagnumOpus.ActiveDirectory;
using MagnumOpus.Dialog;
using MagnumOpus.EditMemberOf;
using MagnumOpus.Group;
using MagnumOpus.Navigation;

namespace MagnumOpus.User
{
    public class UserGroupsViewModel : ViewModelBase
    {
        public UserGroupsViewModel()
        {
            _allGroupsCollectionView = new ListCollectionView(_allGroups);
            _allGroupsCollectionView.SortDescriptions.Add(new SortDescription());
            _allGroupsCollectionView.Filter = TextFilter;

            OpenEditMemberOf = ReactiveCommand.CreateFromObservable(() => _dialogRequests.Handle(new DialogInfo(new EditMemberOfDialog(), _user.Principal.SamAccountName)));

            SaveDirectGroups = ReactiveCommand.CreateFromObservable(() =>
            {
                var saveFileDialog = new SaveFileDialog { Filter = ExcelService.ExcelFileFilter, FileName = _user.CN };
                return saveFileDialog.ShowDialog() ?? false ? ExcelService.SaveGroupsToExcelFile(_directGroups, saveFileDialog.FileName) : Observable.Return(Unit.Default);
            });

            SaveAllGroups = ReactiveCommand.CreateFromObservable(() =>
            {
                var saveFileDialog = new SaveFileDialog { Filter = ExcelService.ExcelFileFilter, FileName = _user.CN };
                return saveFileDialog.ShowDialog() ?? false ? ExcelService.SaveGroupsToExcelFile(_allGroups, saveFileDialog.FileName) : Observable.Return(Unit.Default);
            });

            GetAllGroups = ReactiveCommand.CreateFromObservable(
                () =>
                {
                    AllGroups.Clear();
                    return User.Principal.GetAllGroups(TaskPoolScheduler.Default)
                            .TakeUntil(this.WhenAnyValue(vm => vm.IsShowingAllGroups).Where(false));
                },
                this.WhenAnyValue(vm => vm.IsShowingAllGroups));
            GetAllGroups
                .ObserveOnDispatcher()
                .Select(group =>
                {
                    var cn = group.Properties.Get<string>("cn");
                    group.Dispose();
                    return cn;
                })
                .Subscribe(groupName => AllGroups.Add(groupName));

            _isLoadingGroups = GetAllGroups
                .IsExecuting
                .ToProperty(this, vm => vm.IsLoadingGroups);

            FindDirectGroup = ReactiveCommand.CreateFromTask(() => NavigationService.ShowWindow<GroupWindow>(_selectedDirectGroup));

            FindAllGroup = ReactiveCommand.CreateFromTask(() => NavigationService.ShowWindow<GroupWindow>(_selectedAllGroup));

            this.WhenActivated(disposables =>
            {
                this
                    .WhenAnyValue(vm => vm.GroupFilter, vm => vm.UseFuzzy)
                    .Subscribe(_ => AllGroupsCollectionView?.Refresh())
                    .DisposeWith(disposables);

                GetAllGroups
                    .ThrownExceptions
                    .SelectMany(ex => _messages.Handle(new MessageInfo(MessageType.Error, ex.Message, "Couldn't get groups")))
                    .Subscribe()
                    .DisposeWith(disposables);

                Observable.Merge(
                        this.WhenAnyValue(vm => vm.User).WhereNotNull(),
                        OpenEditMemberOf.Select(_ => User))
                    .Throttle(TimeSpan.FromSeconds(1), RxApp.MainThreadScheduler)
                    .Do(_ => _directGroups.Clear())
                    .SelectMany(user => GetDirectGroups(user.Principal.SamAccountName, TaskPoolScheduler.Default))
                    .Select(group => group.Properties.Get<string>("cn"))
                    .ObserveOnDispatcher()
                    .Subscribe(groupCn => _directGroups.Add(groupCn))
                    .DisposeWith(disposables);

                this
                    .WhenAnyValue(vm => vm.IsShowingDirectGroups)
                    .Where(true)
                    .Subscribe(_ => IsShowingAllGroups = false)
                    .DisposeWith(disposables);

                this
                    .WhenAnyValue(vm => vm.IsShowingAllGroups)
                    .Where(true)
                    .Subscribe(_ => IsShowingDirectGroups = false)
                    .DisposeWith(disposables);

                Observable.Merge<(string Title, string Message)>(
                        OpenEditMemberOf.ThrownExceptions.Select(ex => ("Could not open dialog", ex.Message)),
                        SaveAllGroups.ThrownExceptions.Select(ex => ("Could not save groups", ex.Message)),
                        SaveDirectGroups.ThrownExceptions.Select(ex => ("Could not save groups", ex.Message)),
                        FindDirectGroup.ThrownExceptions.Select(ex => ("Could not open group", ex.Message)),
                        FindAllGroup.ThrownExceptions.Select(ex => ("Could not open group", ex.Message)))
                    .SelectMany(dialogContent => _messages.Handle(new MessageInfo(MessageType.Error, dialogContent.Message, dialogContent.Title)))
                    .Subscribe()
                    .DisposeWith(disposables);
            });
        }



        public ReactiveCommand<Unit, Unit> OpenEditMemberOf { get; }
        public ReactiveCommand<Unit, Unit> SaveDirectGroups { get; }
        public ReactiveCommand<Unit, Unit> SaveAllGroups { get; }
        public ReactiveCommand<Unit, DirectoryEntry> GetAllGroups { get; }
        public ReactiveCommand<Unit, Unit> FindDirectGroup { get; }
        public ReactiveCommand<Unit, Unit> FindAllGroup { get; }
        public ReactiveList<string> AllGroups => _allGroups;
        public IReactiveDerivedList<string> DirectGroups => _directGroups.CreateDerivedCollection(groupName => groupName, orderer: (one, two) => string.Compare(one, two, StringComparison.OrdinalIgnoreCase));
        public ListCollectionView AllGroupsCollectionView => _allGroupsCollectionView;
        public bool IsLoadingGroups => _isLoadingGroups.Value;
        public UserObject User { get => _user; set => this.RaiseAndSetIfChanged(ref _user, value); }
        public bool IsShowingDirectGroups { get => _isShowingDirectGroups; set => this.RaiseAndSetIfChanged(ref _isShowingDirectGroups, value); }
        public bool IsShowingAllGroups { get => _isShowingAllGroups; set => this.RaiseAndSetIfChanged(ref _isShowingAllGroups, value); }
        public string SelectedDirectGroup { get => _selectedDirectGroup; set => this.RaiseAndSetIfChanged(ref _selectedDirectGroup, value); }
        public string SelectedAllGroup { get => _selectedAllGroup; set => this.RaiseAndSetIfChanged(ref _selectedAllGroup, value); }
        public string GroupFilter { get => _groupFilter; set => this.RaiseAndSetIfChanged(ref _groupFilter, value); }
        public bool UseFuzzy { get => _useFuzzy; set => this.RaiseAndSetIfChanged(ref _useFuzzy, value); }



        private IObservable<DirectoryEntry> GetDirectGroups(string identity, IScheduler scheduler = null) => ActiveDirectoryService.Current.GetUser(identity, scheduler)
            .SelectMany(user => user.Principal.GetGroups().ToObservable())
            .Select(group => group.GetUnderlyingObject() as DirectoryEntry);

        bool TextFilter(object item)
        {
            if (!GroupFilter.HasValue()) { return true; }

            var itm = ((string)item).ToLowerInvariant().Replace(" ", "");

            if (UseFuzzy)
            {
                var filterString = GroupFilter.Replace(" ", "").ToLowerInvariant();

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
                return GroupFilter.Split(' ').All(word => itm.Contains(word.ToLowerInvariant()));
            }
        }



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
    }
}
