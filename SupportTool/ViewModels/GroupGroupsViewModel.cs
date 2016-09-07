using ReactiveUI;
using SupportTool.Helpers;
using SupportTool.Models;
using SupportTool.Services.ActiveDirectoryServices;
using SupportTool.Services.NavigationServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupportTool.ViewModels
{
	public class GroupGroupsViewModel : ReactiveObject
	{
		private readonly ReactiveCommand<Unit, Unit> _openAddGroups;
		private readonly ReactiveCommand<Unit, Unit> _openRemoveGroups;
		private readonly ReactiveCommand<Unit, Unit> _findDirectMemberOfGroup;
		private readonly ReactiveList<string> _directMemberOfGroups;
		private GroupObject _group;
		private bool _isShowingDirectMemberOf;
		private bool _isShowingMemberOf;
		private bool _isShowingMembers;
		private object _selectedDirectMemberOfGroup;



		public GroupGroupsViewModel()
		{
			_directMemberOfGroups = new ReactiveList<string>();

			_openAddGroups = ReactiveCommand.CreateFromTask(() => NavigationService.Current.NavigateTo<Views.AddGroupsWindow>(_group.CN));

			_openRemoveGroups = ReactiveCommand.CreateFromTask(() => NavigationService.Current.NavigateTo<Views.RemoveGroupsWindow>(_group.CN));

			_findDirectMemberOfGroup = ReactiveCommand.Create(() => MessageBus.Current.SendMessage(_group.CN, "search"));

			this
				.WhenAnyValue(x => x.IsShowingMemberOf, y => y.IsShowingMembers, (x, y) => x || y)
				.Where(x => x)
				.Subscribe(_ => IsShowingDirectMemberOf = false);
			this
				.WhenAnyValue(x => x.IsShowingDirectMemberOf, y => y.IsShowingMembers, (x, y) => x || y)
				.Where(x => x)
				.Subscribe(_ => IsShowingMemberOf = false);
			this
				.WhenAnyValue(x => x.IsShowingDirectMemberOf, y => y.IsShowingMemberOf, (x, y) => x || y)
				.Where(x => x)
				.Subscribe(_ => IsShowingMembers = false);

			Observable.Merge(
				this.WhenAnyValue(x => x.Group).NotNull(),
				_openAddGroups.Select(_ => _group),
				_openRemoveGroups.Select(_ => _group))
				.Throttle(TimeSpan.FromMilliseconds(500), RxApp.MainThreadScheduler)
				.Do(_ => _directMemberOfGroups.Clear())
				.SelectMany(x => GetDirectGroups(x.CN).SubscribeOn(RxApp.TaskpoolScheduler))
				.ObserveOnDispatcher()
				.Subscribe(x => _directMemberOfGroups.Add(x));
		}



		public ReactiveCommand OpenAddGroups => _openAddGroups;

		public ReactiveCommand OpenRemoveGroups => _openRemoveGroups;

		public ReactiveCommand FindDirectMemberOfGroup => _findDirectMemberOfGroup;

		public ReactiveList<string> DirectMemberOfGroups => _directMemberOfGroups;

		public GroupObject Group
		{
			get { return _group; }
			set { this.RaiseAndSetIfChanged(ref _group, value); }
		}

		public bool IsShowingDirectMemberOf
		{
			get { return _isShowingDirectMemberOf; }
			set { this.RaiseAndSetIfChanged(ref _isShowingDirectMemberOf, value); }
		}

		public bool IsShowingMemberOf
		{
			get { return _isShowingMemberOf; }
			set { this.RaiseAndSetIfChanged(ref _isShowingMemberOf, value); }
		}

		public bool IsShowingMembers
		{
			get { return _isShowingMembers; }
			set { this.RaiseAndSetIfChanged(ref _isShowingMembers, value); }
		}

		public object SelectedDirectMemberOfGroup
		{
			get { return _selectedDirectMemberOfGroup; }
			set { this.RaiseAndSetIfChanged(ref _selectedDirectMemberOfGroup, value); }
		}



		private IObservable<string> GetDirectGroups(string identity) => Observable.Create<string>(observer =>
		{
			var disposed = false;

			var usr = ActiveDirectoryService.Current.GetGroup(identity).Wait();

			foreach (string item in usr.MemberOf)
			{
				var de = ActiveDirectoryService.Current.GetGroups("distinguishedname", item).Take(1).Wait();

				if (disposed) break;
				observer.OnNext(de.Properties.Get<string>("cn"));
			}

			observer.OnCompleted();
			return () => disposed = true;
		});
	}
}
