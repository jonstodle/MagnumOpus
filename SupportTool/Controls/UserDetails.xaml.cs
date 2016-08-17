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
    /// Interaction logic for UserDetails.xaml
    /// </summary>
    public partial class UserDetails : UserControl, IViewFor<UserDetailsViewModel>
    {
        public UserDetails()
        {
            InitializeComponent();

            this.OneWayBind(ViewModel, vm => vm.User.Principal.DisplayName, v => v.DisplayNameTextBlock.Text);
            this.OneWayBind(ViewModel, vm => vm.User.Company, v => v.CompanyRun.Text);
            this.OneWayBind(ViewModel, vm => vm.User.Principal.AccountExpirationDate, v => v.ExpirationRun.Text, x => x != null ? $"Expires {((DateTime)x).ToShortDateString()}" : "Never expires");
            this.OneWayBind(ViewModel, vm => vm.IsAccountLocked, v => v.AccountLockedRun.Text, x => x ? "Locked" : "Not locked");

        }

        public UserObject User
        {
            get { return (UserObject)GetValue(UserProperty); }
            set { SetValue(UserProperty, value); }
        }

        public static readonly DependencyProperty UserProperty = DependencyProperty.Register(nameof(User), typeof(UserObject), typeof(UserDetails), new PropertyMetadata(null, (d,e)=> (d as UserDetails).ViewModel.User = e.NewValue as UserObject));

        public UserDetailsViewModel ViewModel
        {
            get { return (UserDetailsViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(UserDetailsViewModel), typeof(UserDetails), new PropertyMetadata(new UserDetailsViewModel()));

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = value as UserDetailsViewModel; }
        }
    }
}
