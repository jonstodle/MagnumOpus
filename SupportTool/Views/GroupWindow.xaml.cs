using ReactiveUI;
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
using System.Windows.Shapes;

namespace SupportTool.Views
{
	/// <summary>
	/// Interaction logic for GroupWindow.xaml
	/// </summary>
	public partial class GroupWindow : Window, IViewFor<GroupWindowViewModel>
	{
		public GroupWindow()
		{
			InitializeComponent();

			ViewModel = new GroupWindowViewModel();

			this.OneWayBind(ViewModel, vm => vm.Group.CN, v => v.Title, x => x ?? "");
			this.OneWayBind(ViewModel, vm => vm.Group, v => v.GroupDetails.Group);
			this.OneWayBind(ViewModel, vm => vm.Group, v => v.GroupGroups.Group);
		}

		public GroupWindowViewModel ViewModel
		{
			get { return (GroupWindowViewModel)GetValue(ViewModelProperty); }
			set { SetValue(ViewModelProperty, value); }
		}

		public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(GroupWindowViewModel), typeof(GroupWindow), new PropertyMetadata(null));

		object IViewFor.ViewModel
		{
			get { return ViewModel; }
			set { ViewModel = value as GroupWindowViewModel; }
		}
	}
}
