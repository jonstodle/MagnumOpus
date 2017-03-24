using ReactiveUI;
using MagnumOpus.Models;
using MagnumOpus.ViewModels;
using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MagnumOpus.Controls
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
                d(this.Bind(ViewModel, vm => vm.User, v => v.User));

                d(this.OneWayBind(ViewModel, vm => vm.User.Name, v => v.DisplayNameTextBlock.Text));
                d(this.OneWayBind(ViewModel, vm => vm.User.Principal.EmployeeId, v => v.EmployeeIDTextBlock.Text, x => $"({x})"));
                d(this.OneWayBind(ViewModel, vm => vm.User.Principal.SamAccountName, v => v.SamTextBlock.Text, x => x?.ToUpperInvariant()));
                d(this.OneWayBind(ViewModel, vm => vm.User.Company, v => v.CompanyTextBlock.Text, x => x.HasValue() ? x : "No company"));
                d(this.OneWayBind(ViewModel, vm => vm.User.Principal.AccountExpirationDate, v => v.ExpirationTextBlock.Text, x => x != null ? $"User expires {((DateTime)x).ToShortDateString()}" : "User never expires"));
                d(this.OneWayBind(ViewModel, vm => vm.IsAccountLocked, v => v.AccountLockedTextBlock.Text, x => x ? "Locked" : "Not locked"));
                d(this.OneWayBind(ViewModel, vm => vm.User.Principal.Enabled, v => v.AccountEnabledTextBlock.Text, x => x != null ? (bool)x ? "User enabled" : "User disabled" : "Status unavailable"));
                d(this.OneWayBind(ViewModel, vm => vm.PasswordStatus, v => v.PasswordStatusTextBlock.Text));
                d(this.OneWayBind(ViewModel, vm => vm.User.Principal.EmailAddress, v => v.EmailAddressTextBlock.Text, x => x.HasValue() ? x : "No email address"));
                d(this.OneWayBind(ViewModel, vm => vm.IsShowingOrganizationDetails, v => v.OrganizationGrid.Visibility));
                d(this.OneWayBind(ViewModel, vm => vm.User.JobTitle, v => v.JobTitleTextBlock.Text));
                d(this.OneWayBind(ViewModel, vm => vm.User.Department, v => v.DepartmentTextBlock.Text));
                d(this.OneWayBind(ViewModel, vm => vm.User.Company, v => v.CompanyNameTextBlock.Text));
                d(this.OneWayBind(ViewModel, vm => vm.Manager.Name, v => v.ManagerTextBlock.Text));
                d(this.OneWayBind(ViewModel, vm => vm.DirectReports, v => v.DirectReportsListView.ItemsSource));
                d(this.Bind(ViewModel, vm => vm.SelectedDirectReport, v => v.DirectReportsListView.SelectedItem));

                d(this.BindCommand(ViewModel, vm => vm.ToggleOrganizationDetails, v => v.CompanyHyperLink));
                d(this.BindCommand(ViewModel, vm => vm.OpenManager, v => v.ManagerHyperLink));
                d(_directReportsListViewItemDoubleClick.ToEventCommandSignal().InvokeCommand(ViewModel.OpenDirectReport));
                d(this.BindCommand(ViewModel, vm => vm.OpenDirectReport, v => v.OpenDirectReportMenuItem));
            });
        }

        public Interaction<MessageInfo, Unit> InfoMessages => ViewModel.InfoMessages;

        public Interaction<MessageInfo, Unit> ErrorMessages => ViewModel.ErrorMessages;

        public UserObject User { get => (UserObject)GetValue(UserProperty); set => SetValue(UserProperty, value); }
        public static readonly DependencyProperty UserProperty = DependencyProperty.Register(nameof(User), typeof(UserObject), typeof(UserDetails), new PropertyMetadata(null));

        public UserDetailsViewModel ViewModel { get => (UserDetailsViewModel)GetValue(ViewModelProperty); set => SetValue(ViewModelProperty, value); }
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(UserDetailsViewModel), typeof(UserDetails), new PropertyMetadata(null));

        object IViewFor.ViewModel { get => ViewModel; set => ViewModel = value as UserDetailsViewModel; }

        private Subject<MouseButtonEventArgs> _directReportsListViewItemDoubleClick = new Subject<MouseButtonEventArgs>();
        private void DirectReportsListViewItem_DoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e) => _directReportsListViewItemDoubleClick.OnNext(e);
    }
}
