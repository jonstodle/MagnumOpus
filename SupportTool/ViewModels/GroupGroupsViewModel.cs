using ReactiveUI;
using SupportTool.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupportTool.ViewModels
{
	public class GroupGroupsViewModel : ReactiveObject
	{
		private GroupObject _group;
		private bool _isShowingDirectMemberOf;
		private bool _isShowingMemberOf;
		private bool _isShowingMembers;



		public GroupGroupsViewModel()
		{
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
		}



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
	}
}
