using ReactiveUI;
using SupportTool.Models;
using SupportTool.ViewModels;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SupportTool.Controls
{
	/// <summary>
	/// Interaction logic for GroupGroups.xaml
	/// </summary>
	public partial class GroupGroups : UserControl, IViewFor<GroupGroupsViewModel>
	{
		public GroupGroups()
		{
			InitializeComponent();

			ViewModel = new GroupGroupsViewModel();

			this.Bind(ViewModel, vm => vm.IsShowingDirectMemberOf, v => v.DirectMemberOfToggleButton.IsChecked);
			this.OneWayBind(ViewModel, vm => vm.IsShowingDirectMemberOf, v => v.DirectMemberOfGrid.Visibility);
			this.OneWayBind(ViewModel, vm => vm.DirectMemberOfGroupsView, v => v.DirectMemberOfListView.ItemsSource);
			this.Bind(ViewModel, vm => vm.SelectedDirectMemberOfGroup, v => v.DirectMemberOfListView.SelectedItem);

			this.Bind(ViewModel, vm => vm.IsShowingMemberOf, v => v.MemberOfToggleButton.IsChecked);
			this.OneWayBind(ViewModel, vm => vm.IsShowingMemberOf, v => v.MemberOfGrid.Visibility);
			this.Bind(ViewModel, vm => vm.FilterString, v => v.MemberOfFilterTextBox.Text);
			this.Bind(ViewModel, vm => vm.UseFuzzy, v => v.UseFuzzyToggleButton.IsChecked);
			this.OneWayBind(ViewModel, vm => vm.AllMemberOfGroupsView, v => v.MemberOfListView.ItemsSource);
			this.Bind(ViewModel, vm => vm.SelectedAllMemberOfGroup, v => v.MemberOfListView.SelectedItem);
			this.OneWayBind(ViewModel, vm => vm.AllMemberOfGroupsView.Count, v => v.ShowingCountRun.Text);
			this.OneWayBind(ViewModel, vm => vm.AllMemberOfGroups.Count, v => v.TotalCountRun.Text);

			this.Bind(ViewModel, vm => vm.IsShowingMembers, v => v.MembersToggleButton.IsChecked);
			this.OneWayBind(ViewModel, vm => vm.IsShowingMembers, v => v.MembersGrid.Visibility);
			this.OneWayBind(ViewModel, vm => vm.MemberUsersView, v => v.MembersListView.ItemsSource);
			this.Bind(ViewModel, vm => vm.SelectedMemberUser, v => v.MembersListView.SelectedItem);

			this.WhenActivated(d =>
			{
				d(this.BindCommand(ViewModel, vm => vm.FindDirectMemberOfGroup, v => v.DirectMemberOfListView, nameof(ListView.MouseDoubleClick)));
				d(this.BindCommand(ViewModel, vm => vm.OpenEditMemberOf, v => v.EditDirectGroupsButton));
				d(this.BindCommand(ViewModel, vm => vm.SaveDirectGroups, v => v.SaveDirectGroupsButton));
				d(this.BindCommand(ViewModel, vm => vm.FindAllMemberOfGroup, v => v.MemberOfListView, nameof(ListView.MouseDoubleClick)));
				d(this.BindCommand(ViewModel, vm => vm.SaveAllGroups, v => v.SaveAllGroupsButton));
				d(this.BindCommand(ViewModel, vm => vm.OpenEditMembers, v => v.MembersButton));
				d(this.BindCommand(ViewModel, vm => vm.SaveMembers, v => v.SaveMembersButton));
				d(this.BindCommand(ViewModel, vm => vm.FindMemberUser, v => v.MembersListView, nameof(ListView.MouseDoubleClick)));

				d(ViewModel
				.WhenAnyValue(x => x.IsShowingMemberOf)
				.Where(x => x)
				.Select(_ => Unit.Default)
				.ObserveOnDispatcher()
				.InvokeCommand(ViewModel, x => x.GetAllGroups));
			});
		}

		public Interaction<MessageInfo, Unit> InfoMessages => ViewModel.InfoMessages;

		public Interaction<MessageInfo, Unit> ErrorMessages => ViewModel.ErrorMessages;

		public Interaction<DialogInfo, Unit> DialogRequests => ViewModel.DialogRequests;

		public GroupObject Group
		{
			get { return (GroupObject)GetValue(GroupProperty); }
			set { SetValue(GroupProperty, value); }
		}

		public static readonly DependencyProperty GroupProperty = DependencyProperty.Register(nameof(Group), typeof(GroupObject), typeof(GroupGroups), new PropertyMetadata(null, (d,e)=> (d as GroupGroups).ViewModel.Group = e.NewValue as GroupObject));

		public GroupGroupsViewModel ViewModel
		{
			get { return (GroupGroupsViewModel)GetValue(ViewModelProperty); }
			set { SetValue(ViewModelProperty, value); }
		}

		public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(GroupGroupsViewModel), typeof(GroupGroups), new PropertyMetadata(null));

		object IViewFor.ViewModel
		{
			get { return ViewModel; }
			set { ViewModel = value as GroupGroupsViewModel; }
		}
	}
}
