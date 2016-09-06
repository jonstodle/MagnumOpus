using ReactiveUI;
using SupportTool.Helpers;
using SupportTool.Models;
using SupportTool.Services.ActiveDirectoryServices;
using SupportTool.Services.NavigationServices;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.DirectoryServices;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace SupportTool.ViewModels
{
    public class ComputerGroupsViewModel : ReactiveObject
    {
        private readonly ReactiveCommand<Unit, Unit> openAddGroups;
        private readonly ReactiveCommand<Unit, Unit> openRemoveGroups;
		private readonly ReactiveCommand<Unit, Unit> _findDirectGroup;
        private readonly ReactiveList<string> directGroups;
        private readonly ListCollectionView directGroupsCollectionView;
        private ComputerObject computer;
        private bool isShowingDirectGroups;
		private object _selectedDirectGroup;



        public ComputerGroupsViewModel()
        {
            directGroups = new ReactiveList<string>();

            directGroupsCollectionView = new ListCollectionView(directGroups);
            directGroupsCollectionView.SortDescriptions.Add(new SortDescription());

            openAddGroups = ReactiveCommand.CreateFromTask(() => NavigationService.Current.NavigateTo<Views.AddGroupsWindow>(computer.Principal.SamAccountName));

            openRemoveGroups = ReactiveCommand.CreateFromTask(() => NavigationService.Current.NavigateTo<Views.RemoveGroupsWindow>(computer.Principal.SamAccountName));

			_findDirectGroup = ReactiveCommand.Create(() => MessageBus.Current.SendMessage(_selectedDirectGroup as string, "search"));

            Observable.Merge(
                this.WhenAnyValue(x => x.Computer).Where(x => x != null),
                openAddGroups.Select(_ => Computer),
                openRemoveGroups.Select(_ => Computer))
                .Do(_ => DirectGroups.Clear())
                .SelectMany(x => GetDirectGroups(x).SubscribeOn(RxApp.TaskpoolScheduler))
                .ObserveOnDispatcher()
                .Subscribe(x => DirectGroups.Add(x.Properties.Get<string>("cn")));

            this
                .WhenAnyValue(x => x.Computer)
                .Subscribe(_ => ResetValues());
        }



        public ReactiveCommand OpenAddGroups => openAddGroups;

        public ReactiveCommand OpenRemoveGroups => openRemoveGroups;

		public ReactiveCommand FindDirectGroup => _findDirectGroup;

        public ReactiveList<string> DirectGroups => directGroups;

        public ListCollectionView DirectGroupsCollectionView => directGroupsCollectionView;

        public ComputerObject Computer
        {
            get { return computer; }
            set { this.RaiseAndSetIfChanged(ref computer, value); }
        }

        public bool IsShowingDirectGroups
        {
            get { return isShowingDirectGroups; }
            set { this.RaiseAndSetIfChanged(ref isShowingDirectGroups, value); }
        }

		public object SelectedDirectGroup
		{
			get { return _selectedDirectGroup; }
			set { this.RaiseAndSetIfChanged(ref _selectedDirectGroup, value); }
		}



		private void ResetValues()
        {
            directGroups.Clear();
            IsShowingDirectGroups = false;
        }



        private IObservable<DirectoryEntry> GetDirectGroups(ComputerObject computer) => Observable.Create<DirectoryEntry>(async observer =>
        {
            var disposed = false;

            foreach (string item in computer.MemberOf)
            {
                var de = await ActiveDirectoryService.Current.GetGroups("distinguishedname", item).Take(1);

                if (disposed) break;
                observer.OnNext(de);
            }

            observer.OnCompleted();
            return () => disposed = true;
        });
    }
}
