using ReactiveUI;
using Splat;
using MagnumOpus.Models;
using MagnumOpus.Services.ActiveDirectoryServices;
using MagnumOpus.Services.NavigationServices;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Concurrency;

namespace MagnumOpus.ViewModels
{
    public class EditMembersDialogViewModel : ViewModelBase, IDialog, IEnableLogger
    {
        public EditMembersDialogViewModel()
        {
            SetGroup = ReactiveCommand.CreateFromObservable<string, GroupObject>(identity => ActiveDirectoryService.Current.GetGroup(identity));

            GetGroupMembers = ReactiveCommand.CreateFromObservable(() => GetGroupMembersImpl(_group.Value));

            Search = ReactiveCommand.Create(() => ActiveDirectoryService.Current.SearchDirectory(_searchQuery, RxApp.TaskpoolScheduler).Take(1000));

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
                this.WhenAnyValue(x => x.SelectedSearchResult).IsNotNull());

            RemoveFromGroup = ReactiveCommand.Create(
                () =>
                {
                    if (_membersToAdd.Contains(_selectedGroupMember)) _membersToAdd.Remove(_selectedGroupMember);
                    else _membersToRemove.Add(_selectedGroupMember);
                    _groupMembers.Remove(_selectedGroupMember);
                },
                this.WhenAnyValue(x => x.SelectedGroupMember).IsNotNull());

            Save = ReactiveCommand.CreateFromTask(
                async () => await SaveImpl(_group.Value, _membersToAdd, _membersToRemove),
                Observable.CombineLatest(_membersToAdd.CountChanged.StartWith(0), _membersToRemove.CountChanged.StartWith(0), (x, y) => x > 0 || y > 0));

            Cancel = ReactiveCommand.Create(() => _close());

            _group = SetGroup
                .ToProperty(this, x => x.Group);

            (this).WhenActivated((Action<CompositeDisposable>)(disposables =>
            {
                GetGroupMembers
                    .ObserveOnDispatcher()
                .Subscribe(x => _groupMembers.Add(x))
                .DisposeWith(disposables);

                    Search
                    .Do((IObservable<DirectoryEntry> _) => _searchResults.Clear())
                .Switch()
                .ObserveOnDispatcher()
                .Subscribe(x => _searchResults.Add(x))
                .DisposeWith(disposables);

                    Save
                    .SelectMany((IEnumerable<string> x) => x.Count() > 0 ? _messages.Handle(new MessageInfo(MessageType.Warning, $"The following messages were generated:\n{string.Join(Environment.NewLine, x)}")) : Observable.Return(0))
                    .ObserveOnDispatcher()
                    .Do(_ => _close())
                    .Subscribe()
                    .DisposeWith(disposables);

                Observable.Merge(
                        Observable.Select<Exception, (string, string)>(this.SetGroup.ThrownExceptions, (Func<Exception, (string, string)>)(ex => ((string, string))(((string)"Could not load group", (string)ex.Message)))),
                        GetGroupMembers.ThrownExceptions.Select(ex => (("Could not get members", ex.Message))),
                        Search.ThrownExceptions.Select(ex => (("Could not complete search", ex.Message))),
                        OpenSearchResult.ThrownExceptions.Select(ex => (("Could not open AD object", ex.Message))),
                        OpenGroupMember.ThrownExceptions.Select(ex => (("Could not open AD object", ex.Message))),
                        AddToGroup.ThrownExceptions.Select(ex => (("Could not add member", ex.Message))),
                        RemoveFromGroup.ThrownExceptions.Select(ex => (("Could not remove member", ex.Message))),
                        Save.ThrownExceptions.Select(ex => (("Could not save changes", ex.Message))),
                        Cancel.ThrownExceptions.Select(ex => (("Could not close dialog", ex.Message))))
                    .SelectMany(((string, string) x) => _messages.Handle(new MessageInfo(MessageType.Error, x.Item2, x.Item1)))
                    .Subscribe()
                    .DisposeWith(disposables);
            }));
        }



        public ReactiveCommand<string, GroupObject> SetGroup { get; private set; }
        public ReactiveCommand<Unit, DirectoryEntry> GetGroupMembers { get; private set; }
        public ReactiveCommand<Unit, IObservable<DirectoryEntry>> Search { get; private set; }
        public ReactiveCommand<Unit, Unit> OpenSearchResult { get; private set; }
        public ReactiveCommand<Unit, Unit> OpenGroupMember { get; private set; }
        public ReactiveCommand<Unit, Unit> AddToGroup { get; private set; }
        public ReactiveCommand<Unit, Unit> RemoveFromGroup { get; private set; }
        public ReactiveCommand<Unit, IEnumerable<string>> Save { get; private set; }
        public ReactiveCommand<Unit, Unit> Cancel { get; private set; }
        public IReactiveDerivedList<DirectoryEntry> SearchResults => _searchResults.CreateDerivedCollection(x => x, orderer: (one, two) => one.Path.CompareTo(two.Path));
        public IReactiveDerivedList<DirectoryEntry> GroupMembers => _groupMembers.CreateDerivedCollection(x => x, orderer: (one, two) => one.Path.CompareTo(two.Path));
        public ReactiveList<DirectoryEntry> MembersToAdd => _membersToAdd;
        public ReactiveList<DirectoryEntry> MembersToRemove => _membersToRemove;
        public GroupObject Group => _group.Value;
        public string SearchQuery { get => _searchQuery; set => this.RaiseAndSetIfChanged(ref _searchQuery, value); }
        public DirectoryEntry SelectedSearchResult { get => _selectedSearchResult; set => this.RaiseAndSetIfChanged(ref _selectedSearchResult, value); }
        public DirectoryEntry SelectedGroupMember { get => _selectedGroupMember; set => this.RaiseAndSetIfChanged(ref _selectedGroupMember, value); }



        private IObservable<DirectoryEntry> GetGroupMembersImpl(GroupObject group) => Observable.Create<DirectoryEntry>(
            observer =>
                RxApp.TaskpoolScheduler.Schedule(() =>
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
