using ReactiveUI;
using SupportTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
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

namespace SupportTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IViewFor<MainWindowViewModel>
    {
        public MainWindow()
        {
            SupportTool.Services.NavigationServices.NavigationService.Init(this);

            InitializeComponent();



            this.Events()
                .Activated
                .Select(_ => MainTabControl.SelectedIndex)
                .Subscribe(idx =>
                {
                    TextBox tbx = null;

                    if (idx == 0) tbx = UserQueryStringTextBox;
                    else if (idx == 1) tbx = ComputerQueryStringTextBox;

                    tbx?.Focus();
                    tbx?.SelectAll();
                });



            this.Bind(ViewModel, vm => vm.UserQueryString, v => v.UserQueryStringTextBox.Text);
            this.OneWayBind(ViewModel, vm => vm.User, v => v.UserDetailsStackPanel.Visibility, x => x != null ? Visibility.Visible : Visibility.Collapsed);
            this.OneWayBind(ViewModel, vm => vm.User, v => v.UserDetails.User);
            this.OneWayBind(ViewModel, vm => vm.User, v => v.UserPasswordPanel.User);
            this.OneWayBind(ViewModel, vm => vm.User, v => v.UserProfilePanel.User);
            this.OneWayBind(ViewModel, vm => vm.User, v => v.UserGroups.User);

            this.Bind(ViewModel, vm => vm.ComputerQueryString, v => v.ComputerQueryStringTextBox.Text);
            this.OneWayBind(ViewModel, vm => vm.Computer, v => v.ComputerDetailsStackPanel.Visibility, x => x != null ? Visibility.Visible : Visibility.Collapsed);
            this.OneWayBind(ViewModel, vm => vm.Computer, v => v.ComputerDetails.Computer);
            this.OneWayBind(ViewModel, vm => vm.Computer, v => v.PingPanel.Computer);
            this.OneWayBind(ViewModel, vm => vm.Computer, v => v.ComputerGroups.Computer);



            this.WhenActivated(d =>
            {
                d(this.BindCommand(ViewModel, vm => vm.UserPasteAndFind, v => v.UserPasteButton));
                d(this.BindCommand(ViewModel, vm => vm.FindUser, v => v.FindUserButton));

                d(this.BindCommand(ViewModel, vm => vm.ComputerPasteAndFind, v => v.ComputerPasteButton));
                d(this.BindCommand(ViewModel, vm => vm.FindComputer, v => v.FindComputerButton));
            });
        }

        public MainWindowViewModel ViewModel
        {
            get { return (MainWindowViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(MainWindowViewModel), typeof(MainWindow), new PropertyMetadata(new MainWindowViewModel()));

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = value as MainWindowViewModel; }
        }
    }
}
