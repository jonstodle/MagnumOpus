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

        private readonly ReactiveCommand<Unit, IEnumerable<DirectoryEntry>> getAllGroups;
        private readonly ReactiveList<DirectoryEntry> allGroups;
        private readonly ReactiveList<string> directGroups;
        private readonly ListCollectionView collectionView;
        private readonly ObservableAsPropertyHelper<bool> isLoadingGroups;
        private UserObject user;
        private int selectedTabIndex;
        private bool isTabsClicked;
        private string groupFitler;
        private bool useFuzzy;



        public UserGroupsViewModel()
        {
            messages = new Subject<Message>();
            allGroups = new ReactiveList<DirectoryEntry>();
            directGroups = new ReactiveList<string>();

            collectionView = new ListCollectionView(allGroups);
            collectionView.Filter = TextFilter;
            this
                .WhenAnyValue(x => x.GroupFilter, y => y.UseFuzzy)
                .Subscribe(_ => CollectionView?.Refresh());

            getAllGroups = ReactiveCommand.CreateFromObservable(
                () => GetGroupsImpl(User.Principal.SamAccountName)
                        .TakeUntil(this.WhenAnyValue(x => x.IsTabsClicked).Where(x => !x)),
                this.WhenAnyValue(x => x.IsTabsClicked, y => y.AllGroups.Count, (x,y) => x && y == 0));
            getAllGroups
                .Subscribe(x =>
                {
                    using (AllGroups.SuppressChangeNotifications())
                    {
                        AllGroups.Clear();
                        AllGroups.AddRange(x.OrderBy(y => y.Path));
                    }
                });
            getAllGroups
                .ThrownExceptions
                .Subscribe(ex => messages.OnNext(Message.Error(ex.Message, "Couldn't get groups")));
            getAllGroups
                .IsExecuting
                .ToProperty(this, x => x.IsLoadingGroups, out isLoadingGroups);

            this
                .WhenAnyValue(x => x.User)
                .Subscribe(_ => ResetValues());

            this
                .WhenAnyValue(x => x.User)
                .Where(x => x != null)
                .Do(_ => DirectGroups.Clear())
                .SelectMany(x => GetDirectGroups(x))
                .Subscribe(x => DirectGroups.Add(x.Properties.Get<string>("cn")));
        }



        public IObservable<Message> Messages => messages;

        public ReactiveCommand GetAllGroups => getAllGroups;

        public ReactiveList<DirectoryEntry> AllGroups => allGroups;

        public ReactiveList<string> DirectGroups => directGroups;

        public ListCollectionView CollectionView => collectionView;

        public bool IsLoadingGroups => isLoadingGroups.Value;

        public UserObject User
        {
            get { return user; }
            set { this.RaiseAndSetIfChanged(ref user, value); }
        }

        public int SelectedTabIndex
        {
            get { return selectedTabIndex; }
            set { this.RaiseAndSetIfChanged(ref selectedTabIndex, value); }
        }

        public bool IsTabsClicked
        {
            get { return isTabsClicked; }
            set { this.RaiseAndSetIfChanged(ref isTabsClicked, value); }
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
            AllGroups.Clear();
            DirectGroups.Clear();
            SelectedTabIndex = 0;
            IsTabsClicked = false;
            GroupFilter = "";
        }

        private IObservable<DirectoryEntry> GetDirectGroups(UserObject user) => Observable.Create<DirectoryEntry>(async observer =>
        {
            var disposed = false;

            foreach (string item in user.MemberOf)
            {
                var de = (await ActiveDirectoryService.Current.GetGroups("group", "distinguishedname", item)).First().GetDirectoryEntry();

                if (disposed) break;
                observer.OnNext(de);
            }

            observer.OnCompleted();

            return () => disposed = true;
        });

        private IObservable<IEnumerable<DirectoryEntry>> GetGroupsImpl(string samAccountName) => Observable.StartAsync(async () =>
        {
            var result = new List<DirectoryEntry>();

            foreach (var element in User.MemberOf)
            {
                var name = element.ToString();
                result.AddRange(await ActiveDirectoryService.Current.GetParents(name, $"{User.Principal.SamAccountName}/" + name, result));
            }

            return result;
        });

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
