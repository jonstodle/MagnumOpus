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
    /// Interaction logic for UserGroupsWindow.xaml
    /// </summary>
    public partial class UserGroupsWindow : Window, IViewFor<UserGroupsWindowViewModel>
    {
        public UserGroupsWindow()
        {
            InitializeComponent();

            this.OneWayBind(ViewModel, vm => vm.User, v => v.Title, x => x != null ? $"Groups for {x.Principal.DisplayName}" : "");
        }

        public UserGroupsWindowViewModel ViewModel
        {
            get { return (UserGroupsWindowViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(UserGroupsWindowViewModel), typeof(UserGroupsWindow), new PropertyMetadata(new UserGroupsWindowViewModel()));

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = value as UserGroupsWindowViewModel; }
        }
    }
}
