using ReactiveUI;
using SupportTool.Models;
using SupportTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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

			this.Bind(ViewModel, vm => vm.IsShowingDirectMemberOf, v => v.DirectMemberOfToggleButton.IsChecked);
			this.OneWayBind(ViewModel, vm => vm.IsShowingDirectMemberOf, v => v.DirectMemberOfGrid.Visibility);
			this.OneWayBind(ViewModel, vm => vm.DirectMemberOfGroups, v => v.DirectMemberOfListView.ItemsSource);
			this.Bind(ViewModel, vm => vm.SelectedDirectMemberOfGroup, v => v.DirectMemberOfListView.SelectedItem);

			this.Bind(ViewModel, vm => vm.IsShowingMemberOf, v => v.MemberOfToggleButton.IsChecked);
			this.OneWayBind(ViewModel, vm => vm.IsShowingMemberOf, v => v.MemberOfGrid.Visibility);

			this.Bind(ViewModel, vm => vm.IsShowingMembers, v => v.MembersToggleButton.IsChecked);
			this.OneWayBind(ViewModel, vm => vm.IsShowingMembers, v => v.MembersGrid.Visibility);

			this.WhenActivated(d =>
			{
				d(this.BindCommand(ViewModel, vm => vm.FindDirectMemberOfGroup, v => v.DirectMemberOfListView, nameof(ListView.MouseDoubleClick)));
				d(this.BindCommand(ViewModel, vm => vm.OpenAddGroups, v => v.AddToButton));
				d(this.BindCommand(ViewModel, vm => vm.OpenRemoveGroups, v => v.RemoveFromButton));
			});
		}

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

		public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(GroupGroupsViewModel), typeof(GroupGroups), new PropertyMetadata(new GroupGroupsViewModel()));

		object IViewFor.ViewModel
		{
			get { return ViewModel; }
			set { ViewModel = value as GroupGroupsViewModel; }
		}
	}
}
