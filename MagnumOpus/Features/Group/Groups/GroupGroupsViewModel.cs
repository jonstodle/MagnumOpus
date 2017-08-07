using Microsoft.Win32;
using ReactiveUI;
using System;
using System.ComponentModel;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Data;
using System.Reactive.Concurrency;
using DocumentFormat.OpenXml.Spreadsheet;
using MagnumOpus.ActiveDirectory;
using MagnumOpus.Computer;
using MagnumOpus.Dialog;
using MagnumOpus.EditMemberOf;
using MagnumOpus.EditMembers;
using MagnumOpus.Navigation;
using MagnumOpus.User;

namespace MagnumOpus.Group
{
    public class GroupGroupsViewModel : ViewModelBase
    {
        public GroupGroupsViewModel()
        {
            _allMemberOfGroupsView = new ListCollectionView(_allMemberOfGroups)
            {
                Filter = TextFilter,
                SortDescriptions = { new SortDescription() }
            };

            OpenEditMemberOf = ReactiveCommand.CreateFromObservable(() => _dialogRequests.Handle(new DialogInfo(new EditMemberOfDialog(), _group.CN)));

            SaveDirectGroups = ReactiveCommand.CreateFromObservable(() =>
            {
                var saveFileDialog = new SaveFileDialog { Filter = ExcelService.ExcelFileFilter, FileName = _group.CN };
                return saveFileDialog.ShowDialog() ?? false ? ExcelService.SaveGroupsToExcelFile(_directMemberOfGroups, saveFileDialog.FileName) : Observable.Return(Unit.Default);
            });

            FindDirectMemberOfGroup = ReactiveCommand.CreateFromTask(() => NavigationService.ShowWindow<GroupWindow>(_selectedDirectMemberOfGroup));

            GetAllGroups = ReactiveCommand.CreateFromObservable(
                () =>
                {
                    _allMemberOfGroups.Clear();
                    return _group.Principal.GetAllGroups(TaskPoolScheduler.Default)
                                .Select(directoryEntry => directoryEntry.Properties.Get<string>("name"))
                                .TakeUntil(this.WhenAnyValue(vm => vm.IsShowingMemberOf).Where(false));
                },
                this.WhenAnyValue(vm => vm.IsShowingMemberOf));

            FindAllMemberOfGroup = ReactiveCommand.CreateFromTask(() => NavigationService.ShowWindow<GroupWindow>(_selectedAllMemberOfGroup));

            SaveAllGroups = ReactiveCommand.CreateFromObservable(() =>
            {
                var saveFileDialog = new SaveFileDialog { Filter = ExcelService.ExcelFileFilter, FileName = _group.CN };
                return saveFileDialog.ShowDialog() ?? false ? ExcelService.SaveGroupsToExcelFile(_allMemberOfGroups, saveFileDialog.FileName) : Observable.Return(Unit.Default);
            });

            OpenEditMembers = ReactiveCommand.CreateFromObservable(() => _dialogRequests.Handle(new DialogInfo(new EditMembersDialog(), _group.CN)));

            SaveMembers = ReactiveCommand.CreateFromObservable(() =>
            {
                var saveFileDialog = new SaveFileDialog { Filter = ExcelService.ExcelFileFilter, FileName = _group.CN };
                return saveFileDialog.ShowDialog() ?? false ? ExcelService.SaveUsersToExcelFile(_members, saveFileDialog.FileName) : Observable.Return(Unit.Default);
            });

            FindMember = ReactiveCommand.CreateFromTask(() => 
            {
                var principalType = ActiveDirectoryService.Current.DeterminePrincipalType(_selectedMember);
                if (principalType == PrincipalType.Group) return NavigationService.ShowWindow<GroupWindow>(_selectedMember);
                else if (principalType == PrincipalType.Computer) return NavigationService.ShowWindow<ComputerWindow>(_selectedMember);
                else return NavigationService.ShowWindow<UserWindow>(_selectedMember);
            });

            this.WhenActivated(disposables =>
            {
                this
                    .WhenAnyValue(vm => vm.FilterString, y => y.UseFuzzy)
                    .Subscribe(_ => _allMemberOfGroupsView?.Refresh())
                    .DisposeWith(disposables);

                GetAllGroups
                    .ObserveOnDispatcher()
                    .Subscribe(groupName => _allMemberOfGroups.Add(groupName))
                    .DisposeWith(disposables);

                GetAllGroups
                    .ThrownExceptions
                    .SelectMany(ex => _messages.Handle(new MessageInfo(MessageType.Error, ex.Message, "Could not get groups")))
                    .Subscribe()
                    .DisposeWith(disposables);

                this
                    .WhenAnyValue(vm => vm.IsShowingMemberOf, vm => vm.IsShowingMembers, (isShowingMemberOf, isShowingMembers) => isShowingMemberOf || isShowingMembers)
                    .Where(true)
                    .Subscribe(_ => IsShowingDirectMemberOf = false)
                    .DisposeWith(disposables);

                this
                    .WhenAnyValue(vm => vm.IsShowingDirectMemberOf, vm => vm.IsShowingMembers, (isShowingDirectMemberOf, isShowingMembers) => isShowingDirectMemberOf || isShowingMembers)
                    .Where(true)
                    .Subscribe(_ => IsShowingMemberOf = false)
                    .DisposeWith(disposables);

                this
                    .WhenAnyValue(vm => vm.IsShowingDirectMemberOf, vm => vm.IsShowingMemberOf, (isShowingDirectMemberOf, isShowingMemberOf) => isShowingDirectMemberOf || isShowingMemberOf)
                    .Where(true)
                    .Subscribe(_ => IsShowingMembers = false)
                    .DisposeWith(disposables);

                Observable.Merge(
                        this.WhenAnyValue(vm => vm.Group).WhereNotNull(),
                        OpenEditMemberOf.Select(_ => _group))
                    .Throttle(TimeSpan.FromSeconds(1), RxApp.MainThreadScheduler)
                    .Do(_ => _directMemberOfGroups.Clear())
                    .SelectMany(group => GetDirectGroups(group.CN, TaskPoolScheduler.Default))
                    .ObserveOnDispatcher()
                    .Subscribe(cn => _directMemberOfGroups.Add(cn))
                    .DisposeWith(disposables);

                Observable.Merge(
                        this.WhenAnyValue(vm => vm.Group).WhereNotNull(),
                        OpenEditMembers.Select(_ => _group))
                    .Throttle(TimeSpan.FromSeconds(1), RxApp.MainThreadScheduler)
                    .Do(_ => _members.Clear())
                    .SelectMany(group => GetMembers(group.CN, TaskPoolScheduler.Default))
                    .ObserveOnDispatcher()
                    .Subscribe(cn => _members.Add(cn))
                    .DisposeWith(disposables);

                Observable.Merge<(string Title, string Message)>(
                        OpenEditMemberOf.ThrownExceptions.Select(ex => (("Could not open dialog", ex.Message))),
                        SaveDirectGroups.ThrownExceptions.Select(ex => (("Could not save groups", ex.Message))),
                        FindDirectMemberOfGroup.ThrownExceptions.Select(ex => (("Could not open group", ex.Message))),
                        FindAllMemberOfGroup.ThrownExceptions.Select(ex => (("Could not find all groups", ex.Message))),
                        SaveAllGroups.ThrownExceptions.Select(ex => (("Could not save groups", ex.Message))),
                        OpenEditMembers.ThrownExceptions.Select(ex => (("Could not open dialog", ex.Message))),
                        SaveMembers.ThrownExceptions.Select(ex => (("Could not save members", ex.Message))),
                        FindMember.ThrownExceptions.Select(ex => (("Could not open AD object", ex.Message))))
                    .SelectMany(dialogContent => _messages.Handle(new MessageInfo(MessageType.Error, dialogContent.Message, dialogContent.Title)))
                    .Subscribe()
                    .DisposeWith(disposables);
            });
        }



        public ReactiveCommand<Unit, Unit> OpenEditMemberOf { get; }
        public ReactiveCommand<Unit, Unit> SaveDirectGroups { get; }
        public ReactiveCommand<Unit, Unit> FindDirectMemberOfGroup { get; }
        public ReactiveCommand<Unit, string> GetAllGroups { get; }
        public ReactiveCommand<Unit, Unit> FindAllMemberOfGroup { get; }
        public ReactiveCommand<Unit, Unit> SaveAllGroups { get; }
        public ReactiveCommand<Unit, Unit> OpenEditMembers { get; }
        public ReactiveCommand<Unit, Unit> SaveMembers { get; }
        public ReactiveCommand<Unit, Unit> FindMember { get; }
        public IReactiveDerivedList<string> DirectMemberOfGroups => _directMemberOfGroups.CreateDerivedCollection(groupName => groupName, orderer: (one, two) => one.CompareTo(two));
        public ReactiveList<string> AllMemberOfGroups => _allMemberOfGroups;
        public IReactiveDerivedList<string> Members => _members.CreateDerivedCollection(memberName => memberName, orderer: (one, two) => one.CompareTo(two));
        public ListCollectionView AllMemberOfGroupsView => _allMemberOfGroupsView;
        public GroupObject Group { get => _group; set => this.RaiseAndSetIfChanged(ref _group, value); }
        public bool IsShowingDirectMemberOf { get => _isShowingDirectMemberOf; set => this.RaiseAndSetIfChanged(ref _isShowingDirectMemberOf, value); }
        public bool IsShowingMemberOf { get => _isShowingMemberOf; set => this.RaiseAndSetIfChanged(ref _isShowingMemberOf, value); }
        public bool IsShowingMembers { get => _isShowingMembers; set => this.RaiseAndSetIfChanged(ref _isShowingMembers, value); }
        public string SelectedDirectMemberOfGroup { get => _selectedDirectMemberOfGroup; set => this.RaiseAndSetIfChanged(ref _selectedDirectMemberOfGroup, value); }
        public string FilterString { get => _filterString; set => this.RaiseAndSetIfChanged(ref _filterString, value); }
        public bool UseFuzzy { get => _useFuzzy; set => this.RaiseAndSetIfChanged(ref _useFuzzy, value); }
        public string SelectedAllMemberOfGroup { get => _selectedAllMemberOfGroup; set => this.RaiseAndSetIfChanged(ref _selectedAllMemberOfGroup, value); }
        public string SelectedMember { get => _selectedMember; set => this.RaiseAndSetIfChanged(ref _selectedMember, value); }



        private IObservable<string> GetDirectGroups(string identity, IScheduler scheduler = null) => ActiveDirectoryService.Current.GetGroup(identity, scheduler)
            .SelectMany(group => group.Principal.GetGroups().ToObservable())
            .Select(principal => principal.Name);

        private IObservable<string> GetMembers(string identity, IScheduler scheduler = null) => Observable.Create<string>(
            observer =>
                (scheduler ?? TaskPoolScheduler.Default).Schedule(() =>
                    {
                        var group = ActiveDirectoryService.Current.GetGroup(identity).Wait();

                        foreach (Principal item in group.Principal.Members)
                        {
                            observer.OnNext(item.Name);
                        }

                        observer.OnCompleted();
                    }));

        bool TextFilter(object item)
        {
            if (!_filterString.HasValue()) { return true; }

            var itm = ((string)item).ToLowerInvariant().Replace(" ", "");

            if (_useFuzzy)
            {
                var filterString = _filterString.Replace(" ", "").ToLowerInvariant();

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



        private readonly ReactiveList<string> _directMemberOfGroups = new ReactiveList<string>();
        private readonly ReactiveList<string> _allMemberOfGroups = new ReactiveList<string>();
        private readonly ReactiveList<string> _members = new ReactiveList<string>();
        private readonly ListCollectionView _allMemberOfGroupsView;
        private GroupObject _group;
        private bool _isShowingDirectMemberOf;
        private bool _isShowingMemberOf;
        private bool _isShowingMembers;
        private string _selectedDirectMemberOfGroup;
        private string _filterString;
        private bool _useFuzzy;
        private string _selectedAllMemberOfGroup;
        private string _selectedMember;
    }
}
