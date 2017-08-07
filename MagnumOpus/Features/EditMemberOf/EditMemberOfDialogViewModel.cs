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
using System.Threading.Tasks;
using System.Reactive.Concurrency;

namespace MagnumOpus.ViewModels
{
    public class EditMemberOfDialogViewModel : ViewModelBase, IDialog, IEnableLogger
    {
        public EditMemberOfDialogViewModel()
        {
            SetPrincipal = ReactiveCommand.CreateFromObservable<string, Principal>(identity => ActiveDirectoryService.Current.GetPrincipal(identity));

            GetPrincipalMembers = ReactiveCommand.CreateFromObservable(() => GetPrincipalMembersImpl(_principal.Value, TaskPoolScheduler.Default));

            Search = ReactiveCommand.Create(() => ActiveDirectoryService.Current.SearchDirectory(_searchQuery, TaskPoolScheduler.Default).Take(1000));

            OpenSearchResultPrincipal = ReactiveCommand.CreateFromTask(() => NavigateToPrincipal(_selectedSearchResult.Properties.Get<string>("name")));

            OpenMembersPrincipal = ReactiveCommand.CreateFromTask(() => NavigateToPrincipal(_selectedPrincipalMember.Properties.Get<string>("name")));

            AddToPrincipal = ReactiveCommand.Create(
                () =>
                {
                    if (_principalMembers.Contains(_selectedSearchResult) || _membersToAdd.Contains(_selectedSearchResult)) return;
                    _membersToRemove.Remove(_selectedSearchResult);
                    _principalMembers.Add(_selectedSearchResult);
                    _membersToAdd.Add(_selectedSearchResult);
                },
                this.WhenAnyValue(vm => vm.SelectedSearchResult).IsNotNull());

            RemoveFromPrincipal = ReactiveCommand.Create(
                () =>
                {
                    if (_membersToAdd.Contains(_selectedPrincipalMember)) _membersToAdd.Remove(_selectedPrincipalMember);
                    else _membersToRemove.Add(_selectedPrincipalMember);
                    _principalMembers.Remove(_selectedPrincipalMember);
                },
                this.WhenAnyValue(vm => vm.SelectedPrincipalMember).IsNotNull());

            Save = ReactiveCommand.CreateFromTask(
                async () => await SaveImpl(_principal.Value, _membersToAdd, _membersToRemove),
                Observable.CombineLatest(_membersToAdd.CountChanged.StartWith(0), _membersToRemove.CountChanged.StartWith(0), (x, y) => x > 0 || y > 0));

            Cancel = ReactiveCommand.Create(() => _close());

            _principal = SetPrincipal
                .ToProperty(this, vm => vm.Principal);

            (this).WhenActivated((Action<CompositeDisposable>)(disposables =>
            {
                GetPrincipalMembers
                    .ObserveOnDispatcher()
                    .Subscribe(directoryEntry => _principalMembers.Add(directoryEntry))
                    .DisposeWith(disposables);

                Search
                    .Do((IObservable<DirectoryEntry> _) => _searchResults.Clear())
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
                       SetPrincipal.ThrownExceptions.Select(ex => (("Could not load AD object", ex.Message))),
                       GetPrincipalMembers.ThrownExceptions.Select(ex => (("Could not get members", ex.Message))),
                       Search.ThrownExceptions.Select(ex => (("Could not complete search", ex.Message))),
                       OpenSearchResultPrincipal.ThrownExceptions.Select(ex => (("Could not open AD object", ex.Message))),
                       OpenMembersPrincipal.ThrownExceptions.Select(ex => (("Could not open AD object", ex.Message))),
                       AddToPrincipal.ThrownExceptions.Select(ex => (("Could not add member", ex.Message))),
                       RemoveFromPrincipal.ThrownExceptions.Select(ex => (("Could not remove member", ex.Message))),
                       Save.ThrownExceptions.Select(ex => (("Could not save changes", ex.Message))),
                       Cancel.ThrownExceptions.Select(ex => (("Could not close dialog", ex.Message))))
                   .SelectMany(dialogContent => _messages.Handle(new MessageInfo(MessageType.Error, dialogContent.Message, dialogContent.Title)))
                   .Subscribe()
                   .DisposeWith(disposables);
            }));
        }



        public ReactiveCommand<string, Principal> SetPrincipal { get; private set; }
        public ReactiveCommand<Unit, DirectoryEntry> GetPrincipalMembers { get; private set; }
        public ReactiveCommand<Unit, IObservable<DirectoryEntry>> Search { get; private set; }
        public ReactiveCommand<Unit, Unit> OpenSearchResultPrincipal { get; private set; }
        public ReactiveCommand<Unit, Unit> OpenMembersPrincipal { get; private set; }
        public ReactiveCommand<Unit, Unit> AddToPrincipal { get; private set; }
        public ReactiveCommand<Unit, Unit> RemoveFromPrincipal { get; private set; }
        public ReactiveCommand<Unit, IEnumerable<string>> Save { get; private set; }
        public ReactiveCommand<Unit, Unit> Cancel { get; private set; }
        public IReactiveDerivedList<DirectoryEntry> SearchResults => _searchResults.CreateDerivedCollection(directoryEntry => directoryEntry, orderer: (one, two) => one.Path.CompareTo(two.Path));
        public IReactiveDerivedList<DirectoryEntry> PrincipalMembers => _principalMembers.CreateDerivedCollection(directoryEntry => directoryEntry, orderer: (one, two) => one.Path.CompareTo(two.Path));
        public Principal Principal => _principal.Value;
        public string SearchQuery { get => _searchQuery; set => this.RaiseAndSetIfChanged(ref _searchQuery, value); }
        public DirectoryEntry SelectedSearchResult { get => _selectedSearchResult; set => this.RaiseAndSetIfChanged(ref _selectedSearchResult, value); }
        public DirectoryEntry SelectedPrincipalMember { get => _selectedPrincipalMember; set => this.RaiseAndSetIfChanged(ref _selectedPrincipalMember, value); }



        private IObservable<DirectoryEntry> GetPrincipalMembersImpl(Principal principal, IScheduler scheduler = null) => principal.GetGroups()
            .ToObservable(scheduler ?? TaskPoolScheduler.Default)
            .Select(group => group.GetUnderlyingObject() as DirectoryEntry);

        private IObservable<IEnumerable<string>> SaveImpl(Principal principal, IEnumerable<DirectoryEntry> membersToAdd, IEnumerable<DirectoryEntry> membersToRemove) => Observable.Start(() =>
        {
            var result = new List<string>();

            foreach (var groupDe in membersToAdd)
            {
                var groupCN = groupDe.Properties.Get<string>("cn");
                var group = ActiveDirectoryService.Current.GetGroup(groupCN).Wait();

                try
                {
                    if (group == null) throw new NullReferenceException("Not a group");
                    group.Principal.Members.Add(principal);
                    group.Principal.Save();
                    this.Log().Info($"Added \"{groupCN}\" to \"{principal.Name}\"");
                }
                catch (Exception ex)
                {
                    result.Add($"{groupCN} - {ex.Message}");
                    this.Log().Error($"Could not add \"{groupCN}\" to \"{principal.Name}\"");
                }
            }

            foreach (var groupDe in membersToRemove)
            {
                var groupCN = groupDe.Properties.Get<string>("cn");
                var group = ActiveDirectoryService.Current.GetGroup(groupCN).Wait();

                try
                {
                    if (group == null) throw new NullReferenceException("Not a group");
                    group.Principal.Members.Remove(principal);
                    group.Principal.Save();
                    this.Log().Info($"Removed \"{groupCN}\" from \"{principal.Name}\"");
                }
                catch (Exception ex)
                {
                    result.Add($"{groupCN} - {ex.Message}");
                    this.Log().Error($"Could not remove \"{groupCN}\" from \"{principal.Name}\"");
                }
            }

            return result;
        }, TaskPoolScheduler.Default);

        private async Task NavigateToPrincipal(string identity) => await NavigationService.ShowPrincipalWindow(await ActiveDirectoryService.Current.GetPrincipal(identity));



        public Task Opening(Action close, object parameter)
        {
            _close = close;

            if (parameter is string s)
            {
                Observable.Return(s)
                    .InvokeCommand(SetPrincipal);
            }

            return Task.FromResult<object>(null);
        }



        private readonly ReactiveList<DirectoryEntry> _searchResults = new ReactiveList<DirectoryEntry>();
        private readonly ReactiveList<DirectoryEntry> _principalMembers = new ReactiveList<DirectoryEntry>();
        private readonly ReactiveList<DirectoryEntry> _membersToAdd = new ReactiveList<DirectoryEntry>();
        private readonly ReactiveList<DirectoryEntry> _membersToRemove = new ReactiveList<DirectoryEntry>();
        private readonly ObservableAsPropertyHelper<Principal> _principal;
        private string _searchQuery;
        private DirectoryEntry _selectedSearchResult;
        private DirectoryEntry _selectedPrincipalMember;
        private Action _close;
    }
}
