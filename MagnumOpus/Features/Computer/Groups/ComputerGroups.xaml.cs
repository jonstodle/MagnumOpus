using ReactiveUI;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Reactive.Disposables;
using MagnumOpus.Dialog;

namespace MagnumOpus.Computer
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

            this.WhenActivated(d =>
            {
                this.Bind(ViewModel, vm => vm.Computer, v => v.Computer).DisposeWith(d);

                this.Bind(ViewModel, vm => vm.IsShowingDirectGroups, v => v.DirectGroupsToggleButton.IsChecked).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.IsShowingDirectGroups, v => v.DirectGroupsStackPanel.Visibility).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.DirectGroups, v => v.DirectGroupsListView.ItemsSource).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.SelectedDirectGroup, v => v.DirectGroupsListView.SelectedItem).DisposeWith(d);

                _directGroupsListViewItemDoubleClick.ToEventCommandSignal().InvokeCommand(ViewModel.FindDirectGroup).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.FindDirectGroup, v => v.FindDirectGroupMenuItem).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.OpenEditMemberOf, v => v.EditDirectGroupsButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.SaveDirectGroups, v => v.SaveDirectGroupsButton).DisposeWith(d);
            });
        }

        public Interaction<MessageInfo, int> Messages => ViewModel.Messages;

        public Interaction<DialogInfo, Unit> DialogRequests => ViewModel.DialogRequests;

        public ComputerObject Computer { get => (ComputerObject)GetValue(ComputerProperty); set => SetValue(ComputerProperty, value); }
        public static readonly DependencyProperty ComputerProperty = DependencyProperty.Register(nameof(Computer), typeof(ComputerObject), typeof(ComputerGroups), new PropertyMetadata(null));

        public ComputerGroupsViewModel ViewModel { get => (ComputerGroupsViewModel)GetValue(ViewModelProperty); set => SetValue(ViewModelProperty, value); }
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(ComputerGroupsViewModel), typeof(ComputerGroups), new PropertyMetadata(null));

        object IViewFor.ViewModel { get => ViewModel; set => ViewModel = value as ComputerGroupsViewModel; }



        private Subject<MouseButtonEventArgs> _directGroupsListViewItemDoubleClick = new Subject<MouseButtonEventArgs>();
        private void DirectGroupsListViewItem_DoubleClick(object sender, MouseButtonEventArgs e) => _directGroupsListViewItemDoubleClick.OnNext(e);
    }
}
