using ReactiveUI;
using SupportTool.Models;
using SupportTool.ViewModels;
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

namespace SupportTool.Controls
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

            this.Bind(ViewModel, vm => vm.IsShowingNewPasswordOptions, v => v.NewPasswordToggleButton.IsChecked);
            this.OneWayBind(ViewModel, vm => vm.IsShowingNewPasswordOptions, v => v.NewPasswordGrid.Visibility);
            this.Bind(ViewModel, vm => vm.NewPassword, v => v.NewPasswordTextBox.Text);



            this.WhenActivated(d =>
            {
                d(this.BindCommand(ViewModel, vm => vm.SetNewPassword, v => v.SetNewPasswordButton));
                d(this.BindCommand(ViewModel, vm => vm.SetNewSimplePassword, v => v.SetNewSimplePasswordButton));
                d(this.BindCommand(ViewModel, vm => vm.SetNewComplexPassword, v => v.SetNewComplexPasswordButton));
                d(this.BindCommand(ViewModel, vm => vm.ExpirePassword, v => v.ExpirePasswordButton));
                d(this.BindCommand(ViewModel, vm => vm.UnlockAccount, v => v.UnlockAccountButton));
                d(this.BindCommand(ViewModel, vm => vm.RunLockoutStatus, v => v.LockOutStatusButton));
                d(this.BindCommand(ViewModel, vm => vm.OpenSplunk, v => v.SplunkButton));
                d(this.BindCommand(ViewModel, vm => vm.OpenPermittedWorkstations, v => v.PermittedWorkstationsButton));
                d(NewPasswordTextBox.Events()
                    .KeyDown
                    .Where(x => x.Key == Key.Enter)
                    .Select(_ => Unit.Default)
                    .InvokeCommand(ViewModel, x => x.SetNewPassword));
            });
        }

		public Interaction<MessageInfo, Unit> InfoMessages => ViewModel.InfoMessages;

		public Interaction<MessageInfo, Unit> ErrorMessages => ViewModel.ErrorMessages;

		public Interaction<DialogInfo, Unit> DialogRequests => ViewModel.DialogRequests;

		public UserObject User
        {
            get { return (UserObject)GetValue(UserProperty); }
            set { SetValue(UserProperty, value); }
        }

        public static readonly DependencyProperty UserProperty = DependencyProperty.Register(nameof(User), typeof(UserObject), typeof(AccountPanel), new PropertyMetadata(null, (d,e) => (d as AccountPanel).ViewModel.User = e.NewValue as UserObject));

        public AccountPanelViewModel ViewModel
        {
            get { return (AccountPanelViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(AccountPanelViewModel), typeof(AccountPanel), new PropertyMetadata(null));

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = value as AccountPanelViewModel; }
        }
    }
}
