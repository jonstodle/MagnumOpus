using ReactiveUI;
using MagnumOpus.Models;
using MagnumOpus.ViewModels;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Reactive.Disposables;

namespace MagnumOpus.Controls
{
    /// <summary>
    /// Interaction logic for PasswordPanel.xaml
    /// </summary>
    public partial class AccountPanel : UserControl, IViewFor<AccountPanelViewModel>
    {
        public AccountPanel()
        {
            InitializeComponent();

            ViewModel = new AccountPanelViewModel();

            this.WhenActivated(d =>
            {
                this.Bind(ViewModel, vm => vm.User, v => v.User).DisposeWith(d);

                this.Bind(ViewModel, vm => vm.IsShowingNewPasswordOptions, v => v.NewPasswordToggleButton.IsChecked).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.IsShowingNewPasswordOptions, v => v.NewPasswordGrid.Visibility).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.NewPassword, v => v.NewPasswordTextBox.Text).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.User.Principal.Enabled, v => v.ToggleEnabledButton.Content, x => x != null ? (bool)x ? "Disable" : "Enable" : "Unavailable").DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.User.Principal.Enabled, v => v.ToggleEnabledButton.IsEnabled, x => x != null ? true : false).DisposeWith(d);

                this.BindCommand(ViewModel, vm => vm.SetNewPassword, v => v.SetNewPasswordButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.SetNewSimplePassword, v => v.SetNewSimplePasswordButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.SetNewComplexPassword, v => v.SetNewComplexPasswordButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.ExpirePassword, v => v.ExpirePasswordButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.UnlockAccount, v => v.UnlockAccountButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.RunLockoutStatus, v => v.LockOutStatusButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.OpenPermittedWorkstations, v => v.PermittedWorkstationsButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.ToggleEnabled, v => v.ToggleEnabledButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.OpenSplunk, v => v.SplunkButton).DisposeWith(d);
                NewPasswordTextBox.Events()
                    .KeyDown
                    .Where(x => x.Key == Key.Enter)
                    .ToSignal()
                    .InvokeCommand(ViewModel.SetNewPassword).DisposeWith(d);
            });
        }

        public Interaction<MessageInfo, int> Messages => ViewModel.Messages;

        public Interaction<DialogInfo, Unit> DialogRequests => ViewModel.DialogRequests;

        public UserObject User { get => (UserObject)GetValue(UserProperty); set => SetValue(UserProperty, value); }
        public static readonly DependencyProperty UserProperty = DependencyProperty.Register(nameof(User), typeof(UserObject), typeof(AccountPanel), new PropertyMetadata(null));

        public AccountPanelViewModel ViewModel { get => (AccountPanelViewModel)GetValue(ViewModelProperty); set => SetValue(ViewModelProperty, value); }
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(AccountPanelViewModel), typeof(AccountPanel), new PropertyMetadata(null));

        object IViewFor.ViewModel { get => ViewModel; set => ViewModel = value as AccountPanelViewModel; }
    }
}
