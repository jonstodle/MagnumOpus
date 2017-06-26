using Microsoft.Win32;
using ReactiveUI;
using MagnumOpus.Models;
using MagnumOpus.Services.ActiveDirectoryServices;
using MagnumOpus.Services.ExportServices;
using MagnumOpus.Services.NavigationServices;
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

namespace MagnumOpus.ViewModels
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

            OpenEditMemberOf = ReactiveCommand.CreateFromObservable(() => _dialogRequests.Handle(new DialogInfo(new Controls.EditMemberOfDialog(), _group.CN)));

            SaveDirectGroups = ReactiveCommand.CreateFromObservable(() =>
            {
                var saveFileDialog = new SaveFileDialog { Filter = ExcelService.ExcelFileFilter, FileName = _group.CN };
                return saveFileDialog.ShowDialog() ?? false ? ExcelService.SaveGroupsToExcelFile(_directMemberOfGroups, saveFileDialog.FileName) : Observable.Return(Unit.Default);
            });

            FindDirectMemberOfGroup = ReactiveCommand.CreateFromTask(() => NavigationService.ShowWindow<Views.GroupWindow>(_selectedDirectMemberOfGroup));

            GetAllGroups = ReactiveCommand.CreateFromObservable(
                () =>
                {
                    _allMemberOfGroups.Clear();
                    return _group.Principal.GetAllGroups(TaskPoolScheduler.Default)
                                .Select(x => x.Properties.Get<string>("name"))
                                .TakeUntil(this.WhenAnyValue(x => x.IsShowingMemberOf).Where(x => !x));
                },
                this.WhenAnyValue(x => x.IsShowingMemberOf));

            FindAllMemberOfGroup = ReactiveCommand.CreateFromTask(() => NavigationService.ShowWindow<Views.GroupWindow>(_selectedAllMemberOfGroup));

            SaveAllGroups = ReactiveCommand.CreateFromObservable(() =>
            {
                var saveFileDialog = new SaveFileDialog { Filter = ExcelService.ExcelFileFilter, FileName = _group.CN };
                return saveFileDialog.ShowDialog() ?? false ? ExcelService.SaveGroupsToExcelFile(_allMemberOfGroups, saveFileDialog.FileName) : Observable.Return(Unit.Default);
            });

            OpenEditMembers = ReactiveCommand.CreateFromObservable(() => _dialogRequests.Handle(new DialogInfo(new Controls.EditMembersDialog(), _group.CN)));

            SaveMembers = ReactiveCommand.CreateFromObservable(() =>
            {
                var saveFileDialog = new SaveFileDialog { Filter = ExcelService.ExcelFileFilter, FileName = _group.CN };
                return saveFileDialog.ShowDialog() ?? false ? ExcelService.SaveUsersToExcelFile(_members, saveFileDialog.FileName) : Observable.Return(Unit.Default);
            });

            FindMember = ReactiveCommand.CreateFromTask(() => 
            {
                var principalType = ActiveDirectoryService.Current.DeterminePrincipalType(_selectedMember);
                if (principalType == PrincipalType.Group) return NavigationService.ShowWindow<Views.GroupWindow>(_selectedMember);
                else if (principalType == PrincipalType.Computer) return NavigationService.ShowWindow<Views.ComputerWindow>(_selectedMember);
                else return NavigationService.ShowWindow<Views.UserWindow>(_selectedMember);
            });

            this.WhenActivated(disposables =>
            {
                this
                    .WhenAnyValue(x => x.FilterString, y => y.UseFuzzy)
                    .Subscribe((Tuple<string, bool> _) => _allMemberOfGroupsView?.Refresh())
                    .DisposeWith(disposables);

                GetAllGroups
                    .ObserveOnDispatcher()
                    .Subscribe(x => _allMemberOfGroups.Add(x))
                    .DisposeWith(disposables);

                GetAllGroups
                    .ThrownExceptions
                    .SelectMany(ex => _messages.Handle(new MessageInfo(MessageType.Error, ex.Message, "Could not get groups")))
                    .Subscribe()
                    .DisposeWith(disposables);

                this
                    .WhenAnyValue(x => x.IsShowingMemberOf, y => y.IsShowingMembers, (x, y) => x || y)
                    .Where(x => x)
                    .Subscribe(_ => IsShowingDirectMemberOf = false)
                    .DisposeWith(disposables);

                this
                    .WhenAnyValue(x => x.IsShowingDirectMemberOf, y => y.IsShowingMembers, (x, y) => x || y)
                    .Where(x => x)
                    .Subscribe(_ => IsShowingMemberOf = false)
                    .DisposeWith(disposables);

                this
                    .WhenAnyValue(x => x.IsShowingDirectMemberOf, y => y.IsShowingMemberOf, (x, y) => x || y)
                    .Where(x => x)
                    .Subscribe(_ => IsShowingMembers = false)
                    .DisposeWith(disposables);

                Observable.Merge(
                        this.WhenAnyValue(x => x.Group).WhereNotNull(),
                        OpenEditMemberOf.Select(_ => _group))
                    .Throttle(TimeSpan.FromSeconds(1), RxApp.MainThreadScheduler)
                    .Do(_ => _directMemberOfGroups.Clear())
                    .SelectMany(x => GetDirectGroups(x.CN, TaskPoolScheduler.Default))
                    .ObserveOnDispatcher()
                    .Subscribe(x => _directMemberOfGroups.Add(x))
                    .DisposeWith(disposables);

                Observable.Merge(
                        this.WhenAnyValue(x => x.Group).WhereNotNull(),
                        OpenEditMembers.Select(_ => _group))
                    .Throttle(TimeSpan.FromSeconds(1), RxApp.MainThreadScheduler)
                    .Do(_ => _members.Clear())
                    .SelectMany(x => GetMembers(x.CN, TaskPoolScheduler.Default))
                    .ObserveOnDispatcher()
                    .Subscribe(x => _members.Add(x))
                    .DisposeWith(disposables);

                Observable.Merge(
                        OpenEditMemberOf.ThrownExceptions.Select(ex => (("Could not open dialog", ex.Message))),
                        SaveDirectGroups.ThrownExceptions.Select(ex => (("Could not save groups", ex.Message))),
                        FindDirectMemberOfGroup.ThrownExceptions.Select(ex => (("Could not open group", ex.Message))),
                        FindAllMemberOfGroup.ThrownExceptions.Select(ex => (("Could not find all groups", ex.Message))),
                        SaveAllGroups.ThrownExceptions.Select(ex => (("Could not save groups", ex.Message))),
                        OpenEditMembers.ThrownExceptions.Select(ex => (("Could not open dialog", ex.Message))),
                        SaveMembers.ThrownExceptions.Select(ex => (("Could not save members", ex.Message))),
                        FindMember.ThrownExceptions.Select(ex => (("Could not open AD object", ex.Message))))
                    .SelectMany(((string, string) x) => _messages.Handle(new MessageInfo(MessageType.Error, x.Item2, x.Item1)))
                    .Subscribe()
                    .DisposeWith(disposables);
            });
        }



        public ReactiveCommand<Unit, Unit> OpenEditMemberOf { get; private set; }
        public ReactiveCommand<Unit, Unit> SaveDirectGroups { get; private set; }
        public ReactiveCommand<Unit, Unit> FindDirectMemberOfGroup { get; private set; }
        public ReactiveCommand<Unit, string> GetAllGroups { get; private set; }
        public ReactiveCommand<Unit, Unit> FindAllMemberOfGroup { get; private set; }
        public ReactiveCommand<Unit, Unit> SaveAllGroups { get; private set; }
        public ReactiveCommand<Unit, Unit> OpenEditMembers { get; private set; }
        public ReactiveCommand<Unit, Unit> SaveMembers { get; private set; }
        public ReactiveCommand<Unit, Unit> FindMember { get; private set; }
        public IReactiveDerivedList<string> DirectMemberOfGroups => _directMemberOfGroups.CreateDerivedCollection(x => x, orderer: (one, two) => one.CompareTo(two));
        public ReactiveList<string> AllMemberOfGroups => _allMemberOfGroups;
        public IReactiveDerivedList<string> Members => _members.CreateDerivedCollection(x => x, orderer: (one, two) => one.CompareTo(two));
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
            .SelectMany(x => x.Principal.GetGroups().ToObservable())
            .Select(x => x.Name);

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
