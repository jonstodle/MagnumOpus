using ReactiveUI;
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
	public class RemoveUsersWindowViewModel : ReactiveObject, IDialog
	{
		private readonly ReactiveCommand<Unit, Unit> _addToMembersToRemove;
		private readonly ReactiveCommand<Unit, Unit> _removeFromMembersToRemove;
		private readonly ReactiveCommand<Unit, IEnumerable<string>> _save;
		private readonly ReactiveList<DirectoryEntry> _members;
		private readonly ReactiveList<DirectoryEntry> _membersToRemove;
		private readonly ListCollectionView _membersView;
		private readonly ListCollectionView _membersToRemoveView;
		private readonly ObservableAsPropertyHelper<string> _windowTitle;
		private GroupObject _group;
		private object _selectedMember;
		private object _selectedMemberToRemove;
		private Action _close;



		public RemoveUsersWindowViewModel()
		{
			_members = new ReactiveList<DirectoryEntry>();
			_membersToRemove = new ReactiveList<DirectoryEntry>();
			_membersView = new ListCollectionView(_members)
			{
				SortDescriptions = { new SortDescription("Path", ListSortDirection.Ascending) }
			};
			_membersToRemoveView = new ListCollectionView(_membersToRemove)
			{
				SortDescriptions = { new SortDescription("Path", ListSortDirection.Ascending) }
			};

			_addToMembersToRemove = ReactiveCommand.Create(
				() =>
				{
					var sm = _selectedMember as DirectoryEntry;
					if (!_membersToRemove.Contains(sm)) _membersToRemove.Add(sm);
				},
				this.WhenAnyValue(x => x.SelectedMember).Select(x => x != null));

			_removeFromMembersToRemove = ReactiveCommand.Create(
				() => { _membersToRemove.Remove(_selectedMemberToRemove as DirectoryEntry); },
				this.WhenAnyValue(x => x.SelectedMemberToRemove).Select(x => x != null));

			_save = ReactiveCommand.CreateFromObservable(
				() => SaveImpl(_group, _membersToRemove),
				_membersToRemove.CountChanged.Select(x => x > 0));
			_save
				.Take(1)
				.Subscribe(x =>
				{
					if (x.Count() > 0)
					{
						var builder = new StringBuilder();
						builder.AppendLine("The follwoing member(s) were not removed:");
						foreach (var member in x) builder.AppendLine(member);
						DialogService.ShowInfo(builder.ToString(), "Some members were not removed");
					}

					_close();
				});

			_windowTitle = this
				.WhenAnyValue(x => x.Group)
				.WhereNotNull()
				.Select(x => $"Remove users from {x.CN}")
				.ToProperty(this, x => x.WindowTitle, "");

			Observable.Merge(
				_addToMembersToRemove.ThrownExceptions,
				_removeFromMembersToRemove.ThrownExceptions,
				_save.ThrownExceptions)
				.Subscribe(ex => DialogService.ShowError(ex.Message));

			this
				.WhenAnyValue(x => x.Group)
				.WhereNotNull()
				.SelectMany(x => GetMembers(x.CN).SubscribeOn(RxApp.TaskpoolScheduler))
				.ObserveOnDispatcher()
				.Subscribe(x => _members.Add(x));
		}



		public ReactiveCommand AddToMembersToRemove => _addToMembersToRemove;

		public ReactiveCommand RemoveFromMembersToRemove => _removeFromMembersToRemove;

		public ReactiveCommand Save => _save;

		public ReactiveList<DirectoryEntry> Members => _members;

		public ReactiveList<DirectoryEntry> MembersToRemove => _membersToRemove;

		public ListCollectionView MembersView => _membersView;

		public ListCollectionView MembersToRemoveView => _membersToRemoveView;

		public string WindowTitle => _windowTitle.Value;

		public GroupObject Group
		{
			get { return _group; }
			set { this.RaiseAndSetIfChanged(ref _group, value); }
		}

		public object SelectedMember
		{
			get { return _selectedMember; }
			set { this.RaiseAndSetIfChanged(ref _selectedMember, value); }
		}

		public object SelectedMemberToRemove
		{
			get { return _selectedMemberToRemove; }
			set { this.RaiseAndSetIfChanged(ref _selectedMemberToRemove, value); }
		}



		private IObservable<DirectoryEntry> GetMembers(string identity) => Observable.Create<DirectoryEntry>(observer =>
		{
			var disposed = false;

			var group = ActiveDirectoryService.Current.GetGroup(identity).Wait();

			foreach (Principal item in group.Principal.Members)
			{
				if (disposed) break;
				observer.OnNext(item.GetUnderlyingObject() as DirectoryEntry);
			}

			observer.OnCompleted();
			return () => disposed = true;
		});

		private IObservable<IEnumerable<string>> SaveImpl(GroupObject group, IEnumerable<DirectoryEntry> members) => Observable.Start(() =>
		{
			var membersNotRemoved = new List<string>();
			foreach (var memberDe in members)
			{
				var member = ActiveDirectoryService.Current.GetPrincipal(memberDe.Properties.Get<string>("samaccountname")).Wait();

				try { group.Principal.Members.Remove(member); }
				catch { membersNotRemoved.Add(member.DisplayName); }
			}

			group.Principal.Save();

			return membersNotRemoved;
		});



		public async Task Opening(Action close, object parameter)
		{
			_close = close;

			if (parameter is string)
			{
				Group = await ActiveDirectoryService.Current.GetGroup(parameter as string);
			}
		}
	}
}
