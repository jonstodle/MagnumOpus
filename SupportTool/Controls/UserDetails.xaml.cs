using ReactiveUI;
using SupportTool.Models;
using SupportTool.ViewModels;
using System;
using System.Reactive;
using System.Windows;
using System.Windows.Controls;

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

			ViewModel = new UserDetailsViewModel();

            this.OneWayBind(ViewModel, vm => vm.User.Principal.DisplayName, v => v.DisplayNameTextBlock.Text);
			this.OneWayBind(ViewModel, vm => vm.User.Principal.EmployeeId, v => v.EmployeeIDTextBlock.Text, x => $"({x})");
			this.OneWayBind(ViewModel, vm => vm.User.Principal.SamAccountName, v => v.SamTextBlock.Text, x => x?.ToUpperInvariant());
			this.OneWayBind(ViewModel, vm => vm.User.Company, v => v.CompanyTextBlock.Text, x => x.HasValue() ? x : "No company");
            this.OneWayBind(ViewModel, vm => vm.User.Principal.AccountExpirationDate, v => v.ExpirationTextBlock.Text, x => x != null ? $"User expires {((DateTime)x).ToShortDateString()}" : "User never expires");
            this.OneWayBind(ViewModel, vm => vm.IsAccountLocked, v => v.AccountLockedTextBlock.Text, x => x ? "Locked" : "Not locked");
			this.OneWayBind(ViewModel, vm => vm.User.Principal.Enabled, v => v.AccountEnabledTextBlock.Text, x => x != null ? (bool)x ? "User enabled" : "User disabled" : "Status unavailable");
			this.OneWayBind(ViewModel, vm => vm.PasswordAge, v => v.PasswordAgeTextBlock.Text, x => $"Password age: {x.Days}d {x.Hours}h {x.Minutes}m");
			this.OneWayBind(ViewModel, vm => vm.User.Principal.EmailAddress, v => v.EmailAddressTextBlock.Text);
		}

		public Interaction<MessageInfo, Unit> InfoMessages => ViewModel.InfoMessages;

		public Interaction<MessageInfo, Unit> ErrorMessages => ViewModel.ErrorMessages;

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

        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(UserDetailsViewModel), typeof(UserDetails), new PropertyMetadata(null));

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = value as UserDetailsViewModel; }
        }
    }
}
