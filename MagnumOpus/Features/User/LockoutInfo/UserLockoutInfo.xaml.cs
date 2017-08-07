using ReactiveUI;
using System.Windows;
using System.Windows.Controls;
using System.Reactive.Disposables;

namespace MagnumOpus.User
{
    /// <summary>
    /// Interaction logic for UserLockoutInfo.xaml
    /// </summary>
    public partial class UserLockoutInfo : UserControl, IViewFor<UserLockoutInfoViewModel>
    {
        public UserLockoutInfo()
        {
            InitializeComponent();

            this.WhenActivated(d =>
            {
                this.OneWayBind(ViewModel, vm => vm.User.Name, v => v.TitleTextBlock.Text, name => $"Lockout info for {name}").DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.LockoutInfos, v => v.LockoutInfosListView.ItemsSource).DisposeWith(d);

                this.BindCommand(ViewModel, vm => vm.GetLockoutInfo, v => v.RefreshButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.Close, v => v.CloseButton).DisposeWith(d);
            });
        }

        public UserLockoutInfoViewModel ViewModel { get => (UserLockoutInfoViewModel)GetValue(ViewModelProperty); set => SetValue(ViewModelProperty, value); }
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(UserLockoutInfoViewModel), typeof(UserLockoutInfo), new PropertyMetadata(null));

        object IViewFor.ViewModel { get => ViewModel; set => ViewModel = value as UserLockoutInfoViewModel; }
    }
}
