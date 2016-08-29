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
    /// Interaction logic for ComputerGroups.xaml
    /// </summary>
    public partial class ComputerGroups : UserControl, IViewFor<ComputerGroupsViewModel>
    {
        public ComputerGroups()
        {
            InitializeComponent();

            this.Bind(ViewModel, vm => vm.IsShowingDirectGroups, v => v.DirectGroupsToggleButton.IsChecked);
            this.OneWayBind(ViewModel, vm => vm.IsShowingDirectGroups, v => v.DirectGroupsStackPanel.Visibility);
            this.OneWayBind(ViewModel, vm => vm.DirectGroupsCollectionView, v => v.DirectGroupsListView.ItemsSource);

            this.WhenActivated(d =>
            {
                d(this.BindCommand(ViewModel, vm => vm.OpenAddGroups, v => v.AddGroupsButton));
                d(this.BindCommand(ViewModel, vm => vm.OpenRemoveGroups, v => v.RemoveGroupsButton));
            });
        }

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

        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(ComputerGroupsViewModel), typeof(ComputerGroups), new PropertyMetadata(new ComputerGroupsViewModel()));

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = value as ComputerGroupsViewModel; }
        }
    }
}
