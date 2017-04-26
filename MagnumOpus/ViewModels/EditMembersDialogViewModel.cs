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

namespace MagnumOpus.ViewModels
{
    public class EditMembersDialogViewModel : ViewModelBase, IDialog, IEnableLogger
    {
        public EditMembersDialogViewModel()
        {
            _setGroup = ReactiveCommand.CreateFromObservable<string, GroupObject>(identity => ActiveDirectoryService.Current.GetGroup(identity));

            _getGroupMembers = ReactiveCommand.CreateFromObservable(() => GetGroupMembersImpl(_group.Value).SubscribeOn(RxApp.TaskpoolScheduler));

            _search = ReactiveCommand.Create(() => ActiveDirectoryService.Current.SearchDirectory(_searchQuery).Take(1000).SubscribeOn(RxApp.TaskpoolScheduler));

            _openSearchResult = ReactiveCommand.CreateFromTask(() => NavigateToPrincipal(_selectedSearchResult.Properties.Get<string>("name")));

            _openGroupMember = ReactiveCommand.CreateFromTask(() => NavigateToPrincipal(_selectedGroupMember.Properties.Get<string>("name")));

            _addToGroup = ReactiveCommand.Create(
                () =>
                {
                    if (_groupMembers.Contains(_selectedSearchResult) || _membersToAdd.Contains(_selectedSearchResult)) return;
                    _membersToRemove.Remove(_selectedSearchResult);
                    _groupMembers.Add(_selectedSearchResult);
                    _membersToAdd.Add(_selectedSearchResult);
                },
                this.WhenAnyValue(x => x.SelectedSearchResult).IsNotNull());

            _removeFromGroup = ReactiveCommand.Create(
                () =>
                {
                    if (_membersToAdd.Contains(_selectedGroupMember)) _membersToAdd.Remove(_selectedGroupMember);
                    else _membersToRemove.Add(_selectedGroupMember);
                    _groupMembers.Remove(_selectedGroupMember);
                },
                this.WhenAnyValue(x => x.SelectedGroupMember).IsNotNull());

            _save = ReactiveCommand.CreateFromTask(
                async () => await SaveImpl(_group.Value, _membersToAdd, _membersToRemove),
                Observable.CombineLatest(_membersToAdd.CountChanged.StartWith(0), _membersToRemove.CountChanged.StartWith(0), (x, y) => x > 0 || y > 0));

            _cancel = ReactiveCommand.Create(() => _close());

            _group = _setGroup
                .ToProperty(this, x => x.Group);

            this.WhenActivated(disposables =>
            {
            _getGroupMembers
                .ObserveOnDispatcher()
                .Subscribe(x => _groupMembers.Add(x))
                .DisposeWith(disposables);

            _search
                .Do(_ => _searchResults.Clear())
                .Switch()
                .ObserveOnDispatcher()
                .Subscribe(x => _searchResults.Add(x))
                .DisposeWith(disposables);

            _save
                .SelectMany(x => x.Count() > 0 ? _messages.Handle(new MessageInfo(MessageType.Warning, $"The following messages were generated:\n{string.Join(Environment.NewLine, x)}")) : Observable.Return(0))
                    .ObserveOnDispatcher()
                    .Do(_ => _close())
                    .Subscribe()
                    .DisposeWith(disposables);

                Observable.Merge(
                        _setGroup.ThrownExceptions.Select(ex => ("Could not load group", ex.Message)),
                        _getGroupMembers.ThrownExceptions.Select(ex => ("Could not get members", ex.Message)),
                        _search.ThrownExceptions.Select(ex => ("Could not complete search", ex.Message)),
                        _openSearchResult.ThrownExceptions.Select(ex => ("Could not open AD object", ex.Message)),
                        _openGroupMember.ThrownExceptions.Select(ex => ("Could not open AD object", ex.Message)),
                        _addToGroup.ThrownExceptions.Select(ex => ("Could not add member", ex.Message)),
                        _removeFromGroup.ThrownExceptions.Select(ex => ("Could not remove member", ex.Message)),
                        _save.ThrownExceptions.Select(ex => ("Could not save changes", ex.Message)),
                        _cancel.ThrownExceptions.Select(ex => ("Could not close dialog", ex.Message)))
                    .SelectMany(x => _messages.Handle(new MessageInfo(MessageType.Error, x.Item2, x.Item1)))
                    .Subscribe()
                    .DisposeWith(disposables);
            });
        }



        public ReactiveCommand SetGroup => _setGroup;

        public ReactiveCommand GetGroupMembers => _getGroupMembers;

        public ReactiveCommand Search => _search;

        public ReactiveCommand OpenSearchResult => _openSearchResult;

        public ReactiveCommand OpenGroupMember => _openGroupMember;

        public ReactiveCommand AddToGroup => _addToGroup;

        public ReactiveCommand RemoveFromGroup => _removeFromGroup;

        public ReactiveCommand Save => _save;

        public ReactiveCommand Cancel => _cancel;

        public IReactiveDerivedList<DirectoryEntry> SearchResults => _searchResults.CreateDerivedCollection(x => x, orderer: (one, two) => one.Path.CompareTo(two.Path));

        public IReactiveDerivedList<DirectoryEntry> GroupMembers => _groupMembers.CreateDerivedCollection(x => x, orderer: (one, two) => one.Path.CompareTo(two.Path));

        public ReactiveList<DirectoryEntry> MembersToAdd => _membersToAdd;

        public ReactiveList<DirectoryEntry> MembersToRemove => _membersToRemove;

        public GroupObject Group => _group.Value;

        public string SearchQuery { get => _searchQuery; set => this.RaiseAndSetIfChanged(ref _searchQuery, value); }

        public DirectoryEntry SelectedSearchResult { get => _selectedSearchResult; set => this.RaiseAndSetIfChanged(ref _selectedSearchResult, value); }

        public DirectoryEntry SelectedGroupMember { get => _selectedGroupMember; set => this.RaiseAndSetIfChanged(ref _selectedGroupMember, value); }



        private IObservable<DirectoryEntry> GetGroupMembersImpl(GroupObject group) => Observable.Create<DirectoryEntry>(observer =>
        {
            var disposed = false;

            foreach (Principal item in group.Principal.Members)
            {
                if (disposed) break;
                observer.OnNext(item.GetUnderlyingObject() as DirectoryEntry);
            }

            observer.OnCompleted();
            return () => disposed = true;
        });

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
        });

        private async Task NavigateToPrincipal(string identity) => await NavigationService.ShowPrincipalWindow(await ActiveDirectoryService.Current.GetPrincipal(identity));



        public Task Opening(Action close, object parameter)
        {
            _close = close;

            if (parameter is string s)
            {
                Observable.Return(s)
                    .InvokeCommand(_setGroup);
            }

            return Task.FromResult<object>(null);
        }



        private readonly ReactiveCommand<string, GroupObject> _setGroup;
        private readonly ReactiveCommand<Unit, DirectoryEntry> _getGroupMembers;
        private readonly ReactiveCommand<Unit, IObservable<DirectoryEntry>> _search;
        private readonly ReactiveCommand<Unit, Unit> _openSearchResult;
        private readonly ReactiveCommand<Unit, Unit> _openGroupMember;
        private readonly ReactiveCommand<Unit, Unit> _addToGroup;
        private readonly ReactiveCommand<Unit, Unit> _removeFromGroup;
        private readonly ReactiveCommand<Unit, IEnumerable<string>> _save;
        private readonly ReactiveCommand<Unit, Unit> _cancel;
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
