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

            _openEditMemberOf = ReactiveCommand.CreateFromTask(async () => await _dialogRequests.Handle(new DialogInfo(new Controls.EditMemberOfDialog(), _group.CN)));

            _saveDirectGroups = ReactiveCommand.CreateFromTask(async () =>
            {
                var saveFileDialog = new SaveFileDialog { Filter = ExcelService.ExcelFileFilter };
                if (saveFileDialog.ShowDialog() == true)
                {
                    await ExcelService.SaveGroupsToExcelFile(_directMemberOfGroups, saveFileDialog.FileName);
                }
            });

            _findDirectMemberOfGroup = ReactiveCommand.CreateFromTask(() => NavigationService.ShowWindow<Views.GroupWindow>(_selectedDirectMemberOfGroup));

            _getAllMemberOfGroups = ReactiveCommand.CreateFromObservable(
                () =>
                {
                    _allMemberOfGroups.Clear();
                    return _group.Principal.GetAllGroups().SubscribeOn(RxApp.TaskpoolScheduler)
                                .Select(x => x.Properties.Get<string>("name"))
                                .TakeUntil(this.WhenAnyValue(x => x.IsShowingMemberOf).Where(x => !x));
                },
                this.WhenAnyValue(x => x.IsShowingMemberOf));

            _findAllMemberOfGroup = ReactiveCommand.CreateFromTask(() => NavigationService.ShowWindow<Views.GroupWindow>(_selectedAllMemberOfGroup));

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
                    await ExcelService.SaveUsersToExcelFile(_members, saveFileDialog.FileName);
                }
            });

            _findMember = ReactiveCommand.CreateFromTask(() => 
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
                    .Subscribe(_ => _allMemberOfGroupsView?.Refresh())
                    .DisposeWith(disposables);

                _getAllMemberOfGroups
                    .ObserveOnDispatcher()
                    .Subscribe(x => _allMemberOfGroups.Add(x))
                    .DisposeWith(disposables);

                _getAllMemberOfGroups
                    .ThrownExceptions
                    .SelectMany(ex => _errorMessages.Handle(new MessageInfo(MessageType.Error, ex.Message, "Couldn't get groups")))
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
                    _openEditMemberOf.Select(_ => _group))
                    .Throttle(TimeSpan.FromSeconds(1), RxApp.MainThreadScheduler)
                    .Do(_ => _directMemberOfGroups.Clear())
                    .SelectMany(x => GetDirectGroups(x.CN).SubscribeOn(RxApp.TaskpoolScheduler))
                    .ObserveOnDispatcher()
                    .Subscribe(x => _directMemberOfGroups.Add(x))
                    .DisposeWith(disposables);

                Observable.Merge(
                    this.WhenAnyValue(x => x.Group).WhereNotNull(),
                    _openEditMembers.Select(_ => _group))
                    .Throttle(TimeSpan.FromSeconds(1), RxApp.MainThreadScheduler)
                    .Do(_ => _members.Clear())
                    .SelectMany(x => GetMembers(x.CN).SubscribeOn(RxApp.TaskpoolScheduler))
                    .ObserveOnDispatcher()
                    .Subscribe(x => _members.Add(x))
                    .DisposeWith(disposables);

                Observable.Merge(
                    _openEditMemberOf.ThrownExceptions,
                    _saveDirectGroups.ThrownExceptions,
                    _findDirectMemberOfGroup.ThrownExceptions,
                    _findAllMemberOfGroup.ThrownExceptions,
                    _saveAllGroups.ThrownExceptions,
                    _openEditMembers.ThrownExceptions,
                    _saveMembers.ThrownExceptions,
                    _findMember.ThrownExceptions)
                    .SelectMany(ex => _errorMessages.Handle(new MessageInfo(MessageType.Error, ex.Message)))
                    .Subscribe()
                    .DisposeWith(disposables);
            });
        }



        public ReactiveCommand OpenEditMemberOf => _openEditMemberOf;

        public ReactiveCommand SaveDirectGroups => _saveDirectGroups;

        public ReactiveCommand FindDirectMemberOfGroup => _findDirectMemberOfGroup;

        public ReactiveCommand GetAllGroups => _getAllMemberOfGroups;

        public ReactiveCommand FindAllMemberOfGroup => _findAllMemberOfGroup;

        public ReactiveCommand SaveAllGroups => _saveAllGroups;

        public ReactiveCommand OpenEditMembers => _openEditMembers;

        public ReactiveCommand SaveMembers => _saveMembers;

        public ReactiveCommand FindMember => _findMember;
    
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



        private IObservable<string> GetDirectGroups(string identity) => ActiveDirectoryService.Current.GetGroup(identity)
            .SelectMany(x => x.Principal.GetGroups().ToObservable())
            .Select(x => x.Name);

        private IObservable<string> GetMembers(string identity) => Observable.Create<string>(observer =>
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



        private readonly ReactiveCommand<Unit, Unit> _openEditMemberOf;
        private readonly ReactiveCommand<Unit, Unit> _saveDirectGroups;
        private readonly ReactiveCommand<Unit, Unit> _findDirectMemberOfGroup;
        private readonly ReactiveCommand<Unit, string> _getAllMemberOfGroups;
        private readonly ReactiveCommand<Unit, Unit> _findAllMemberOfGroup;
        private readonly ReactiveCommand<Unit, Unit> _saveAllGroups;
        private readonly ReactiveCommand<Unit, Unit> _openEditMembers;
        private readonly ReactiveCommand<Unit, Unit> _saveMembers;
        private readonly ReactiveCommand<Unit, Unit> _findMember;
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
