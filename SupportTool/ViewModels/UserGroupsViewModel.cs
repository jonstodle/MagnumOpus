using ReactiveUI;
using SupportTool.Helpers;
using SupportTool.Models;
using SupportTool.Services.ActiveDirectoryServices;
using SupportTool.Services.NavigationServices;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace SupportTool.ViewModels
{
    public class UserGroupsViewModel : ReactiveObject
    {
        private readonly Subject<Message> messages;

        private readonly ReactiveCommand<Unit, IEnumerable<DirectoryEntry>> getGroups;
        private readonly ReactiveList<DirectoryEntry> groups;
        private readonly ListCollectionView collectionView;
        private readonly ObservableAsPropertyHelper<bool> isLoadingGroups;
        private UserObject user;
        private bool isShowingUserGroups;
        private string groupFitler;
        private bool useFuzzy;



        public UserGroupsViewModel()
        {
            messages = new Subject<Message>();
            groups = new ReactiveList<DirectoryEntry>();

            collectionView = new ListCollectionView(groups);
            collectionView.Filter = TextFilter;
            this
                .WhenAnyValue(x => x.GroupFilter, y => y.UseFuzzy)
                .Subscribe(_ => CollectionView?.Refresh());

            getGroups = ReactiveCommand.CreateFromTask(
                () => GetGroupsImpl(User.Principal.SamAccountName),
                this.WhenAnyValue(x => x.IsShowingUserGroups, y => y.Groups.Count, (x,y) => x && y == 0));
            getGroups
                .Subscribe(x =>
                {
                    using (Groups.SuppressChangeNotifications())
                    {
                        Groups.Clear();
                        Groups.AddRange(x.OrderBy(y => y.Path));
                    }
                });
            getGroups
                .ThrownExceptions
                .Subscribe(ex => messages.OnNext(Message.Error(ex.Message, "Couldn't get groups")));
            getGroups
                .IsExecuting
                .ToProperty(this, x => x.IsLoadingGroups, out isLoadingGroups);

            this
                .WhenAnyValue(x => x.User)
                .Subscribe(_ => ResetValues());
        }



        public IObservable<Message> Messages => messages;

        public ReactiveCommand GetGroups => getGroups;

        public ReactiveList<DirectoryEntry> Groups => groups;

        public ListCollectionView CollectionView => collectionView;

        public bool IsLoadingGroups => isLoadingGroups.Value;

        public UserObject User
        {
            get { return user; }
            set { this.RaiseAndSetIfChanged(ref user, value); }
        }

        public bool IsShowingUserGroups
        {
            get { return isShowingUserGroups; }
            set { this.RaiseAndSetIfChanged(ref isShowingUserGroups, value); }
        }

        public string GroupFilter
        {
            get { return groupFitler; }
            set { this.RaiseAndSetIfChanged(ref groupFitler, value); }
        }

        public bool UseFuzzy
        {
            get { return useFuzzy; }
            set { this.RaiseAndSetIfChanged(ref useFuzzy, value); }
        }



        private void ResetValues()
        {
            Groups.Clear();
            IsShowingUserGroups = false;
        }

        private async Task<IEnumerable<DirectoryEntry>> GetGroupsImpl(string samAccountName)
        {
            var result = new List<DirectoryEntry>();

            foreach (var element in User.MemberOf)
            {
                var name = element.ToString();
                result.AddRange(await ActiveDirectoryService.Current.GetParents(name, $"{User.Principal.SamAccountName}/" + name, result));
            }

            return result;
        }

        bool TextFilter(object item)
        {
            if (!GroupFilter.HasValue()) { return true; }

            var itm = ((DirectoryEntry)item).Properties["cn"][0].ToString().ToLowerInvariant().Replace(" ", string.Empty);

            itm = itm.ToLowerInvariant();

            if (UseFuzzy)
            {
                var filterString = GroupFilter.Replace(" ", string.Empty).ToLowerInvariant();

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
                if (itm.Contains(GroupFilter.ToLowerInvariant()))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
