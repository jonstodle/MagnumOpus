using ReactiveUI;
using SupportTool.Models;
using SupportTool.ViewModels;
using System.Reactive;
using System.Windows;
using System.Windows.Controls;

namespace SupportTool.Controls
{
	/// <summary>
	/// Interaction logic for ComputerGroups.xaml
	/// </summary>
	public partial class ComputerGroups : UserControl, IViewFor<ComputerGroupsViewModel>
    {
        public ComputerGroups()
        {
            InitializeComponent();

			ViewModel = new ComputerGroupsViewModel();

            this.Bind(ViewModel, vm => vm.IsShowingDirectGroups, v => v.DirectGroupsToggleButton.IsChecked);
            this.OneWayBind(ViewModel, vm => vm.IsShowingDirectGroups, v => v.DirectGroupsStackPanel.Visibility);
            this.OneWayBind(ViewModel, vm => vm.DirectGroupsCollectionView, v => v.DirectGroupsListView.ItemsSource);
			this.Bind(ViewModel, vm => vm.SelectedDirectGroup, v => v.DirectGroupsListView.SelectedItem);

			this.WhenActivated(d =>
            {
				d(this.BindCommand(ViewModel, vm => vm.FindDirectGroup, v => v.DirectGroupsListView, nameof(ListView.MouseDoubleClick)));
				d(this.BindCommand(ViewModel, vm => vm.OpenEditMemberOf, v => v.EditDirectGroupsButton));
				d(this.BindCommand(ViewModel, vm => vm.SaveDirectGroups, v => v.SaveDirectGroupsButton));
			});
		}

		public Interaction<MessageInfo, Unit> InfoMessages => ViewModel.InfoMessages;

		public Interaction<MessageInfo, Unit> ErrorMessages => ViewModel.ErrorMessages;

		public Interaction<DialogInfo, Unit> DialogRequests => ViewModel.DialogRequests;

		public ComputerObject Computer
        {
            get { return (ComputerObject)GetValue(ComputerProperty); }
            set { SetValue(ComputerProperty, value); }
        }

        public static readonly DependencyProperty ComputerProperty = DependencyProperty.Register(nameof(Computer), typeof(ComputerObject), typeof(ComputerGroups), new PropertyMetadata(null, (d, e) => (d as ComputerGroups).ViewModel.Computer = e.NewValue as ComputerObject));

        public ComputerGroupsViewModel ViewModel
        {
            get { return (ComputerGroupsViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(ComputerGroupsViewModel), typeof(ComputerGroups), new PropertyMetadata(null));

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = value as ComputerGroupsViewModel; }
        }
    }
}
