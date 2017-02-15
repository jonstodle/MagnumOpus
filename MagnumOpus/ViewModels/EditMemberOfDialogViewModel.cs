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
    public class EditMemberOfDialogViewModel : ViewModelBase, IDialog, IEnableLogger
    {
        public EditMemberOfDialogViewModel()
        {
            _setPrincipal = ReactiveCommand.CreateFromObservable<string, Principal>(identity => ActiveDirectoryService.Current.GetPrincipal(identity));

            _getPrincipalMembers = ReactiveCommand.CreateFromObservable(() => GetPrincipalMembersImpl(_principal.Value).SubscribeOn(RxApp.TaskpoolScheduler));

            _search = ReactiveCommand.Create(() => ActiveDirectoryService.Current.SearchDirectory(_searchQuery).Take(1000).SubscribeOn(RxApp.TaskpoolScheduler));

            _openSearchResultPrincipal = ReactiveCommand.CreateFromTask(() => NavigateToPrincipal(_selectedSearchResult.Properties.Get<string>("name")));

            _openMembersPrincipal = ReactiveCommand.CreateFromTask(() => NavigateToPrincipal(_selectedPrincipalMember.Properties.Get<string>("name")));

            _addToPrincipal = ReactiveCommand.Create(
                () =>
                {
                    if (_principalMembers.Contains(_selectedSearchResult) || _membersToAdd.Contains(_selectedSearchResult)) return;
                    _membersToRemove.Remove(_selectedSearchResult);
                    _principalMembers.Add(_selectedSearchResult);
                    _membersToAdd.Add(_selectedSearchResult);
                },
                this.WhenAnyValue(x => x.SelectedSearchResult).IsNotNull());

            _removeFromPrincipal = ReactiveCommand.Create(
                () =>
                {
                    if (_membersToAdd.Contains(_selectedPrincipalMember)) _membersToAdd.Remove(_selectedPrincipalMember);
                    else _membersToRemove.Add(_selectedPrincipalMember);
                    _principalMembers.Remove(_selectedPrincipalMember);
                },
                this.WhenAnyValue(x => x.SelectedPrincipalMember).IsNotNull());

            _save = ReactiveCommand.CreateFromTask(
                async () => await SaveImpl(_principal.Value, _membersToAdd, _membersToRemove),
                Observable.CombineLatest(_membersToAdd.CountChanged.StartWith(0), _membersToRemove.CountChanged.StartWith(0), (x, y) => x > 0 || y > 0));

            _cancel = ReactiveCommand.Create(() => _close());

            _principal = _setPrincipal
                .ToProperty(this, x => x.Principal);

            this.WhenActivated(disposables =>
            {
                _getPrincipalMembers
                    .ObserveOnDispatcher()
                    .Subscribe(x => _principalMembers.Add(x))
                    .DisposeWith(disposables);

                _search
                    .Do(_ => _searchResults.Clear())
                    .Switch()
                    .ObserveOnDispatcher()
                    .Subscribe(x => _searchResults.Add(x))
                    .DisposeWith(disposables);

                _save
                    .Subscribe(async x =>
                    {
                        if (x.Count() > 0)
                        {
                            var builder = new StringBuilder();
                            foreach (var message in x) builder.AppendLine(message);
                            await _infoMessages.Handle(new MessageInfo($"The following messages were generated:\n{builder.ToString()}"));
                        }

                        _close();
                    })
                    .DisposeWith(disposables);

                Observable.Merge(
                   _setPrincipal.ThrownExceptions,
                   _getPrincipalMembers.ThrownExceptions,
                   _search.ThrownExceptions,
                   _openSearchResultPrincipal.ThrownExceptions,
                   _openMembersPrincipal.ThrownExceptions,
                   _addToPrincipal.ThrownExceptions,
                   _removeFromPrincipal.ThrownExceptions,
                   _save.ThrownExceptions,
                   _cancel.ThrownExceptions)
               .Subscribe(async ex => await _errorMessages.Handle(new MessageInfo(ex.Message)))
               .DisposeWith(disposables);
            });
        }



        public ReactiveCommand SetPrincipal => _setPrincipal;

        public ReactiveCommand GetPrincipalMembers => _getPrincipalMembers;

        public ReactiveCommand Search => _search;

        public ReactiveCommand OpenSearchResultPrincipal => _openSearchResultPrincipal;

        public ReactiveCommand OpenMembersPrincipal => _openMembersPrincipal;

        public ReactiveCommand AddToPrincipal => _addToPrincipal;

        public ReactiveCommand RemoveFromPrincipal => _removeFromPrincipal;

        public ReactiveCommand Save => _save;

        public ReactiveCommand Cancel => _cancel;

        public IReactiveDerivedList<DirectoryEntry> SearchResults => _searchResults.CreateDerivedCollection(x => x, orderer: (one, two) => one.Path.CompareTo(two.Path));

        public IReactiveDerivedList<DirectoryEntry> PrincipalMembers => _principalMembers.CreateDerivedCollection(x => x, orderer: (one, two) => one.Path.CompareTo(two.Path));

        public Principal Principal => _principal.Value;

        public string SearchQuery { get => _searchQuery; set => this.RaiseAndSetIfChanged(ref _searchQuery, value); }

        public DirectoryEntry SelectedSearchResult { get => _selectedSearchResult; set => this.RaiseAndSetIfChanged(ref _selectedSearchResult, value); }

        public DirectoryEntry SelectedPrincipalMember { get => _selectedPrincipalMember; set => this.RaiseAndSetIfChanged(ref _selectedPrincipalMember, value); }



        private IObservable<DirectoryEntry> GetPrincipalMembersImpl(Principal principal) => principal.GetGroups()
            .ToObservable()
            .Select(x => x.GetUnderlyingObject() as DirectoryEntry);

        private IObservable<IEnumerable<string>> SaveImpl(Principal principal, IEnumerable<DirectoryEntry> membersToAdd, IEnumerable<DirectoryEntry> membersToRemove) => Observable.Start(() =>
        {
            var result = new List<string>();

            foreach (var groupDe in membersToAdd)
            {
                var group = ActiveDirectoryService.Current.GetGroup(groupDe.Properties.Get<string>("cn")).Wait();

                try
                {
                    if (group == null) throw new NullReferenceException("Not a group");
                    group.Principal.Members.Add(principal);
                    group.Principal.Save();
                    this.Log().Info($"Added \"{ group.CN}\" to \"{principal.Name}\"");
                }
                catch (Exception ex)
                {
                    result.Add($"{group.CN} - {ex.Message}");
                    this.Log().Error($"Could not add \"{ group.CN}\" to \"{principal.Name}\"");
                }
            }

            foreach (var groupDe in membersToRemove)
            {
                var group = ActiveDirectoryService.Current.GetGroup(groupDe.Properties.Get<string>("cn")).Wait();

                try
                {
                    if (group == null) throw new NullReferenceException("Not a group");
                    group.Principal.Members.Remove(principal);
                    group.Principal.Save();
                    this.Log().Info($"Removed \"{ group.CN}\" from \"{principal.Name}\"");
                }
                catch (Exception ex)
                {
                    result.Add($"{group.CN} - {ex.Message}");
                    this.Log().Error($"Could not remove \"{ group.CN}\" from \"{principal.Name}\"");
                }
            }

            return result;
        });

        private async Task NavigateToPrincipal(string identity) => await NavigationService.ShowPrincipalWindow(await ActiveDirectoryService.Current.GetPrincipal(identity));



        public Task Opening(Action close, object parameter)
        {
            _close = close;

            if (parameter is string)
            {
                Observable.Return(parameter as string)
                    .InvokeCommand(_setPrincipal);
            }

            return Task.FromResult<object>(null);
        }



        private readonly ReactiveCommand<string, Principal> _setPrincipal;
        private readonly ReactiveCommand<Unit, DirectoryEntry> _getPrincipalMembers;
        private readonly ReactiveCommand<Unit, IObservable<DirectoryEntry>> _search;
        private readonly ReactiveCommand<Unit, Unit> _openSearchResultPrincipal;
        private readonly ReactiveCommand<Unit, Unit> _openMembersPrincipal;
        private readonly ReactiveCommand<Unit, Unit> _addToPrincipal;
        private readonly ReactiveCommand<Unit, Unit> _removeFromPrincipal;
        private readonly ReactiveCommand<Unit, IEnumerable<string>> _save;
        private readonly ReactiveCommand<Unit, Unit> _cancel;
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
