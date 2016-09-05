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
    /// Interaction logic for GroupDetails.xaml
    /// </summary>
    public partial class GroupDetails : UserControl, IViewFor<GroupDetailsViewModel>
    {
        public GroupDetails()
        {
            InitializeComponent();

            this.OneWayBind(ViewModel, vm => vm.Group.CN, v => v.CNTextBlock.Text);
        }

        public GroupObject Group
        {
            get { return (GroupObject)GetValue(GroupProperty); }
            set { SetValue(GroupProperty, value); }
        }

        public static readonly DependencyProperty GroupProperty = DependencyProperty.Register(nameof(Group), typeof(GroupObject), typeof(GroupDetails), new PropertyMetadata(null, (d, e) => (d as GroupDetails).ViewModel.Group = e.NewValue as GroupObject));

        public GroupDetailsViewModel ViewModel
        {
            get { return (GroupDetailsViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(GroupDetailsViewModel), typeof(GroupDetails), new PropertyMetadata(new GroupDetailsViewModel()));

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = value as GroupDetailsViewModel; }
        }
    }
}
