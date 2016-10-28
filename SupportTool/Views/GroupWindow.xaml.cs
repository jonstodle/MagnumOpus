using ReactiveUI;
using SupportTool.ViewModels;
using System.Windows;

namespace SupportTool.Views
{
	/// <summary>
	/// Interaction logic for GroupWindow.xaml
	/// </summary>
	public partial class GroupWindow : DetailsWindow, IViewFor<GroupWindowViewModel>
	{
		public GroupWindow()
		{
			InitializeComponent();

			ViewModel = new GroupWindowViewModel();

			this.OneWayBind(ViewModel, vm => vm.Group.CN, v => v.Title, x => x ?? "");
			this.OneWayBind(ViewModel, vm => vm.Group, v => v.GroupDetails.Group);
			this.OneWayBind(ViewModel, vm => vm.Group, v => v.GroupGroups.Group);

			this.WhenActivated(d =>
			{
				d(this.BindCommand(ViewModel, vm => vm.SetGroup, v => v.RefreshHyperlink, ViewModel.WhenAnyValue(x => x.Group.CN)));
			});
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
