using ReactiveUI;
using Splat;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Reactive.Concurrency;
using MagnumOpus.ActiveDirectory;
using MagnumOpus.Dialog;
using MagnumOpus.Group;
using MagnumOpus.Navigation;

namespace MagnumOpus.EditMembers
{
    public class EditMembersDialogViewModel : ViewModelBase, IDialog, IEnableLogger
    {
        public EditMembersDialogViewModel()
        {
            SetGroup = ReactiveCommand.CreateFromObservable<string, GroupObject>(identity => ActiveDirectoryService.Current.GetGroup(identity));

            GetGroupMembers = ReactiveCommand.CreateFromObservable(() => GetGroupMembersImpl(_group.Value));

            Search = ReactiveCommand.Create(() => ActiveDirectoryService.Current.SearchDirectory(_searchQuery, TaskPoolScheduler.Default).Take(1000));

            OpenSearchResult = ReactiveCommand.CreateFromTask(() => NavigateToPrincipal(_selectedSearchResult.Properties.Get<string>("name")));

            OpenGroupMember = ReactiveCommand.CreateFromTask(() => NavigateToPrincipal(_selectedGroupMember.Properties.Get<string>("name")));

            AddToGroup = ReactiveCommand.Create(
                () =>
                {
                    if (_groupMembers.Contains(_selectedSearchResult) || _membersToAdd.Contains(_selectedSearchResult)) return;
                    _membersToRemove.Remove(_selectedSearchResult);
                    _groupMembers.Add(_selectedSearchResult);
                    _membersToAdd.Add(_selectedSearchResult);
                },
                this.WhenAnyValue(vm => vm.SelectedSearchResult).IsNotNull());

            RemoveFromGroup = ReactiveCommand.Create(
                () =>
                {
                    if (_membersToAdd.Contains(_selectedGroupMember)) _membersToAdd.Remove(_selectedGroupMember);
                    else _membersToRemove.Add(_selectedGroupMember);
                    _groupMembers.Remove(_selectedGroupMember);
                },
                this.WhenAnyValue(vm => vm.SelectedGroupMember).IsNotNull());

            Save = ReactiveCommand.CreateFromTask(
                async () => await SaveImpl(_group.Value, _membersToAdd, _membersToRemove),
                Observable.CombineLatest(_membersToAdd.CountChanged.StartWith(0), _membersToRemove.CountChanged.StartWith(0), (x, y) => x > 0 || y > 0));

            Cancel = ReactiveCommand.Create(() => _close());

            _group = SetGroup
                .ToProperty(this, vm => vm.Group);

            (this).WhenActivated((Action<CompositeDisposable>)(disposables =>
            {
                GetGroupMembers
                    .ObserveOnDispatcher()
                    .Subscribe(directoryEntry => _groupMembers.Add(directoryEntry))
                    .DisposeWith(disposables);

                Search
                    .Do(_ => _searchResults.Clear())
                    .Switch()
                    .ObserveOnDispatcher()
                    .Subscribe(directoryEntry => _searchResults.Add(directoryEntry))
                    .DisposeWith(disposables);

                Save
                    .SelectMany((IEnumerable<string> x) => x.Count() > 0 ? _messages.Handle(new MessageInfo(MessageType.Warning, $"The following messages were generated:\n{string.Join(Environment.NewLine, x)}")) : Observable.Return(0))
                    .ObserveOnDispatcher()
                    .Do(_ => _close())
                    .Subscribe()
                    .DisposeWith(disposables);

                Observable.Merge<(string Title, string Message)>(
                        SetGroup.ThrownExceptions.Select(ex => (("Could not load group", ex.Message))),
                        GetGroupMembers.ThrownExceptions.Select(ex => (("Could not get members", ex.Message))),
                        Search.ThrownExceptions.Select(ex => (("Could not complete search", ex.Message))),
                        OpenSearchResult.ThrownExceptions.Select(ex => (("Could not open AD object", ex.Message))),
                        OpenGroupMember.ThrownExceptions.Select(ex => (("Could not open AD object", ex.Message))),
                        AddToGroup.ThrownExceptions.Select(ex => (("Could not add member", ex.Message))),
                        RemoveFromGroup.ThrownExceptions.Select(ex => (("Could not remove member", ex.Message))),
                        Save.ThrownExceptions.Select(ex => (("Could not save changes", ex.Message))),
                        Cancel.ThrownExceptions.Select(ex => (("Could not close dialog", ex.Message))))
                    .SelectMany(dialogContent => _messages.Handle(new MessageInfo(MessageType.Error, dialogContent.Message, dialogContent.Title)))
                    .Subscribe()
                    .DisposeWith(disposables);
            }));
        }



        public ReactiveCommand<string, GroupObject> SetGroup { get; }
        public ReactiveCommand<Unit, DirectoryEntry> GetGroupMembers { get; }
        public ReactiveCommand<Unit, IObservable<DirectoryEntry>> Search { get; }
        public ReactiveCommand<Unit, Unit> OpenSearchResult { get; }
        public ReactiveCommand<Unit, Unit> OpenGroupMember { get; }
        public ReactiveCommand<Unit, Unit> AddToGroup { get; }
        public ReactiveCommand<Unit, Unit> RemoveFromGroup { get; }
        public ReactiveCommand<Unit, IEnumerable<string>> Save { get; }
        public ReactiveCommand<Unit, Unit> Cancel { get; }
        public IReactiveDerivedList<DirectoryEntry> SearchResults => _searchResults.CreateDerivedCollection(directoryEntry => directoryEntry, orderer: (one, two) => one.Path.CompareTo(two.Path));
        public IReactiveDerivedList<DirectoryEntry> GroupMembers => _groupMembers.CreateDerivedCollection(directoryEntry => directoryEntry, orderer: (one, two) => one.Path.CompareTo(two.Path));
        public ReactiveList<DirectoryEntry> MembersToAdd => _membersToAdd;
        public ReactiveList<DirectoryEntry> MembersToRemove => _membersToRemove;
        public GroupObject Group => _group.Value;
        public string SearchQuery { get => _searchQuery; set => this.RaiseAndSetIfChanged(ref _searchQuery, value); }
        public DirectoryEntry SelectedSearchResult { get => _selectedSearchResult; set => this.RaiseAndSetIfChanged(ref _selectedSearchResult, value); }
        public DirectoryEntry SelectedGroupMember { get => _selectedGroupMember; set => this.RaiseAndSetIfChanged(ref _selectedGroupMember, value); }



        private IObservable<DirectoryEntry> GetGroupMembersImpl(GroupObject group) => Observable.Create<DirectoryEntry>(
            observer =>
                TaskPoolScheduler.Default.Schedule(() =>
                    {
                        foreach (Principal item in group.Principal.Members)
                        {
                            observer.OnNext(item.GetUnderlyingObject() as DirectoryEntry);
                        }

                        observer.OnCompleted();
                    }));

        private IObservable<IEnumerable<string>> SaveImpl(GroupObject group, IEnumerable<DirectoryEntry> membersToAdd, IEnumerable<DirectoryEntry> membersToRemove) => Observable.Start(() =>
        {
            var result = new List<string>();

            foreach (var memberDe in membersToAdd)
            {
                var member = ActiveDirectoryService.Current.GetPrincipal(memberDe.Properties.Get<string>("samaccountname")).Wait();

                try
                {
                    group.Principal.Members.Add(member);
                    this.Log().Info($"Added \"{ member.Name}\" to \"{group.CN}\"");
                }
                catch (Exception ex)
                {
                    result.Add($"{member.SamAccountName} - {ex.Message}");
                    this.Log().Error($"Could not add \"{ member.Name}\" to \"{group.CN}\"");
                }
            }

            foreach (var memberDe in membersToRemove)
            {
                var member = ActiveDirectoryService.Current.GetPrincipal(memberDe.Properties.Get<string>("samaccountname")).Wait();

                try
                {
                    group.Principal.Members.Remove(member);
                    this.Log().Info($"Removed \"{ member.Name}\" from \"{group.CN}\"");
                }
                catch (Exception ex)
                {
                    result.Add($"{member.SamAccountName} - {ex.Message}");
                    this.Log().Error($"Could not remove \"{ member.Name}\" from \"{group.CN}\"");
                }
            }

            group.Principal.Save();

            return result;
        }, TaskPoolScheduler.Default);

        private async Task NavigateToPrincipal(string identity) => await NavigationService.ShowPrincipalWindow(await ActiveDirectoryService.Current.GetPrincipal(identity));



        public Task Opening(Action close, object parameter)
        {
            _close = close;

            if (parameter is string s)
            {
                Observable.Return(s)
                    .InvokeCommand(SetGroup);
            }

            return Task.FromResult<object>(null);
        }



        private readonly ReactiveList<DirectoryEntry> _searchResults = new ReactiveList<DirectoryEntry>();
        private readonly ReactiveList<DirectoryEntry> _groupMembers = new ReactiveList<DirectoryEntry>();
        private readonly ReactiveList<DirectoryEntry> _membersToAdd = new ReactiveList<DirectoryEntry>();
        private readonly ReactiveList<DirectoryEntry> _membersToRemove = new ReactiveList<DirectoryEntry>();
        private readonly ObservableAsPropertyHelper<GroupObject> _group;
        private string _searchQuery;
        private DirectoryEntry _selectedSearchResult;
        private DirectoryEntry _selectedGroupMember;
        private Action _close;
    }
}
