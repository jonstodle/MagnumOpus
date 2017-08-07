using ReactiveUI;
using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Reactive.Disposables;
using MagnumOpus.Dialog;

namespace MagnumOpus.User
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

            this.WhenActivated(d =>
            {
                this.Bind(ViewModel, vm => vm.User, v => v.User).DisposeWith(d);

                this.OneWayBind(ViewModel, vm => vm.User.Name, v => v.DisplayNameTextBlock.Text).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.User.Principal.EmployeeId, v => v.EmployeeIDTextBlock.Text, employeeId => $"({employeeId})").DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.User.Principal.SamAccountName, v => v.SamTextBlock.Text, samAccountName => samAccountName?.ToUpperInvariant()).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.User.Company, v => v.CompanyTextBlock.Text, company => company.HasValue() ? company : "No company").DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.User.Principal.AccountExpirationDate, v => v.ExpirationTextBlock.Text, expirationDate => expirationDate != null ? $"User expires {((DateTime)expirationDate).ToShortDateString()}" : "User never expires").DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.IsAccountLocked, v => v.AccountLockedTextBlock.Text, isLocked => isLocked ? "Locked" : "Not locked").DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.User.Principal.Enabled, v => v.AccountEnabledTextBlock.Text, isEnabled => isEnabled != null ? (bool)isEnabled ? "User enabled" : "User disabled" : "Status unavailable").DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.PasswordStatus, v => v.PasswordStatusTextBlock.Text).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.PasswordMaxAge, v => v.PasswordMaxAgeTextBlock.Text, duration => duration != null && duration > TimeSpan.Zero ? $" ({duration.TotalDays}d)" : "").DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.User.Principal.EmailAddress, v => v.EmailAddressTextBlock.Text, emailAddress => emailAddress.HasValue() ? emailAddress : "No email address").DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.IsShowingOrganizationDetails, v => v.OrganizationGrid.Visibility).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.User.JobTitle, v => v.JobTitleTextBlock.Text).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.User.Department, v => v.DepartmentTextBlock.Text).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.User.Company, v => v.CompanyNameTextBlock.Text).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.Manager.Name, v => v.ManagerTextBlock.Text).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.DirectReports, v => v.DirectReportsListView.ItemsSource).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.SelectedDirectReport, v => v.DirectReportsListView.SelectedItem).DisposeWith(d);

                this.BindCommand(ViewModel, vm => vm.ToggleOrganizationDetails, v => v.CompanyHyperLink).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.OpenManager, v => v.ManagerHyperLink).DisposeWith(d);
                _directReportsListViewItemDoubleClick.ToEventCommandSignal().InvokeCommand(ViewModel.OpenDirectReport).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.OpenDirectReport, v => v.OpenDirectReportMenuItem).DisposeWith(d);
            });
        }

        public Interaction<MessageInfo, int> Messages => ViewModel.Messages;

        public UserObject User { get => (UserObject)GetValue(UserProperty); set => SetValue(UserProperty, value); }
        public static readonly DependencyProperty UserProperty = DependencyProperty.Register(nameof(User), typeof(UserObject), typeof(UserDetails), new PropertyMetadata(null));

        public UserDetailsViewModel ViewModel { get => (UserDetailsViewModel)GetValue(ViewModelProperty); set => SetValue(ViewModelProperty, value); }
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(UserDetailsViewModel), typeof(UserDetails), new PropertyMetadata(null));

        object IViewFor.ViewModel { get => ViewModel; set => ViewModel = value as UserDetailsViewModel; }

        private Subject<MouseButtonEventArgs> _directReportsListViewItemDoubleClick = new Subject<MouseButtonEventArgs>();
        private void DirectReportsListViewItem_DoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e) => _directReportsListViewItemDoubleClick.OnNext(e);
    }
}
