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
    /// Interaction logic for PasswordPanel.xaml
    /// </summary>
    public partial class PasswordPanel : UserControl, IViewFor<PasswordPanelViewModel>
    {
        public PasswordPanel()
        {
            InitializeComponent();

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
            });
        }

        public IObservable<Message> Messages => ViewModel.Messages;

        public UserObject User
        {
            get { return (UserObject)GetValue(UserProperty); }
            set { SetValue(UserProperty, value); }
        }

        public static readonly DependencyProperty UserProperty = DependencyProperty.Register(nameof(User), typeof(UserObject), typeof(PasswordPanel), new PropertyMetadata(null, (d,e) => (d as PasswordPanel).ViewModel.User = e.NewValue as UserObject));

        public PasswordPanelViewModel ViewModel
        {
            get { return (PasswordPanelViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(PasswordPanelViewModel), typeof(PasswordPanel), new PropertyMetadata(new PasswordPanelViewModel()));

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = value as PasswordPanelViewModel; }
        }
    }
}
