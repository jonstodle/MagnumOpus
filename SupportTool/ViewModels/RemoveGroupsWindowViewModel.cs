using ReactiveUI;
using SupportTool.Helpers;
using SupportTool.Models;
using SupportTool.Services.ActiveDirectoryServices;
using SupportTool.Services.DialogServices;
using SupportTool.Services.NavigationServices;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace SupportTool.ViewModels
{
    public class RemoveGroupsWindowViewModel : ReactiveObject, IDialog
    {
        private readonly ReactiveCommand<Unit, Unit> addGroupToGroupsToRemove;
        private readonly ReactiveCommand<Unit, bool> removeGroupFromGroupsToRemove;
        private readonly ReactiveCommand<Unit, IEnumerable<string>> removePrincipalFromGroups;
        private readonly ReactiveList<DirectoryEntry> principalGroups;
        private readonly ReactiveList<DirectoryEntry> groupsToRemove;
        private readonly ListCollectionView principalGroupsView;
        private readonly ListCollectionView groupsToRemoveView;
        private readonly ObservableAsPropertyHelper<string> windowTitle;
        private Principal principal;
        private object selectedPrincipalGroup;
        private object selectedGroupToRemove;
		private Action _close;



        public RemoveGroupsWindowViewModel()
        {
            principalGroups = new ReactiveList<DirectoryEntry>();
            groupsToRemove = new ReactiveList<DirectoryEntry>();

            principalGroupsView = new ListCollectionView(principalGroups)
            {
                SortDescriptions = { new SortDescription(nameof(DirectoryEntry.Path), ListSortDirection.Ascending) }
            };

            groupsToRemoveView = new ListCollectionView(groupsToRemove)
            {
                SortDescriptions = { new SortDescription(nameof(DirectoryEntry.Path), ListSortDirection.Ascending) }
            };

            addGroupToGroupsToRemove = ReactiveCommand.Create(
                () => groupsToRemove.Add(SelectedPrincipalGroup as DirectoryEntry),
                this.WhenAnyValue(x => x.SelectedPrincipalGroup).Select(x => x != null));
            addGroupToGroupsToRemove
                .ThrownExceptions
                .Subscribe(ex => DialogService.ShowError(ex.Message));

            removeGroupFromGroupsToRemove = ReactiveCommand.Create(
                () => groupsToRemove.Remove(selectedGroupToRemove as DirectoryEntry),
                this.WhenAnyValue(x => x.SelectedGroupToRemove).Select(x => x != null));
            removeGroupFromGroupsToRemove
                .ThrownExceptions
                .Subscribe(ex => DialogService.ShowError(ex.Message));

            removePrincipalFromGroups = ReactiveCommand.CreateFromObservable(
                () => RemovePrincipalFromGroupsImpl(groupsToRemove, principal),
                this.WhenAnyObservable(x => x.groupsToRemove.CountChanged).Select(x => x > 0));
            removePrincipalFromGroups
                .Take(1)
                .Subscribe(x =>
                {
                    if (x.Count() > 0)
                    {
                        var builder = new StringBuilder();
                        builder.AppendLine("The follwoing group(s) were not removed:");
                        foreach (var group in x) builder.AppendLine(group);
                        DialogService.ShowInfo(builder.ToString(), "Some groups were not removed");
                    }

                    MessageBus.Current.SendMessage(ApplicationActionRequest.LoadDirectGroupsForUser);
                    _close();
                });
            removePrincipalFromGroups
                .ThrownExceptions
                .Subscribe(ex => DialogService.ShowError(ex.Message, "Could not remove groups"));

            var principalChanged = this
                .WhenAnyValue(x => x.Principal)
                .NotNull();

            principalChanged
                .Select(x => $"Remove groups from {x.DisplayName}")
                .ToProperty(this, x => x.WindowTitle, out windowTitle);

            principalChanged
                .SelectMany(x => GetMemberOfDirectoryEntries(x).SubscribeOn(RxApp.TaskpoolScheduler))
                .ObserveOnDispatcher()
                .Subscribe(x => principalGroups.Add(x));
        }



        public ReactiveCommand AddGroupToGroupsToRemove => addGroupToGroupsToRemove;

        public ReactiveCommand RemoveGroupFromGroupsToRemove => removeGroupFromGroupsToRemove;

        public ReactiveCommand RemovePrincipalFromGroups => removePrincipalFromGroups;

        public ReactiveList<DirectoryEntry> PrincipalGroups => principalGroups;

        public ReactiveList<DirectoryEntry> GroupsToRemove => groupsToRemove;

        public ListCollectionView PrincipalGroupsView => principalGroupsView;

        public ListCollectionView GroupsToRemoveView => groupsToRemoveView;

        public string WindowTitle => windowTitle.Value;

        public Principal Principal
        {
            get { return principal; }
            set { this.RaiseAndSetIfChanged(ref principal, value); }
        }

        public object SelectedPrincipalGroup
        {
            get { return selectedPrincipalGroup; }
            set { this.RaiseAndSetIfChanged(ref selectedPrincipalGroup, value); }
        }

        public object SelectedGroupToRemove
        {
            get { return selectedGroupToRemove; }
            set { this.RaiseAndSetIfChanged(ref selectedGroupToRemove, value); }
        }


        private void ResetValues()
        {
            PrincipalGroups.Clear();
            GroupsToRemove.Clear();
        }



        private IObservable<DirectoryEntry> GetMemberOfDirectoryEntries(Principal principal) => Observable.Create<DirectoryEntry>(observer =>
        {
            var disposed = false;

            var adObject = new ActiveDirectoryObject<Principal>(principal);

            foreach (string memberof in adObject.MemberOf)
            {
                var group = ActiveDirectoryService.Current.GetGroups("distinguishedname", memberof).Take(1).Wait();
                if (disposed) break;
                observer.OnNext(group);
            }

            observer.OnCompleted();
            return () => disposed = true;
        });

        private IObservable<IEnumerable<string>> RemovePrincipalFromGroupsImpl(IEnumerable<DirectoryEntry> groups, Principal principal) => Observable.Start(() =>
        {
            var groupsNotRemoved = new List<string>();
            foreach (var groupDe in groups)
            {
                var group = ActiveDirectoryService.Current.GetGroup(groupDe.Properties.Get<string>("cn")).Wait();

                try { group.Principal.Members.Remove(principal); }
                catch { groupsNotRemoved.Add(group.CN); }

                group.Principal.Save();
            }

            return groupsNotRemoved;
        });



		public async Task Opening(Action close, object parameter)
		{
			_close = close;

			ResetValues();

			if (parameter is string)
			{
				Principal = await ActiveDirectoryService.Current.GetPrincipal(parameter as string);
			}
		}
	}
}
