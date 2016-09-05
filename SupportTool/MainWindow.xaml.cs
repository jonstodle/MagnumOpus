using ReactiveUI;
using SupportTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
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
                .Subscribe(_ =>
                {
                    QueryStringTextBox.Focus();
                    QueryStringTextBox.SelectAll();
                });



            this.Bind(ViewModel, vm => vm.QueryString, v => v.QueryStringTextBox.Text);

            this.OneWayBind(ViewModel, vm => vm.User, v => v.UserDetailsStackPanel.Visibility, x => x != null ? Visibility.Visible : Visibility.Collapsed);
            this.OneWayBind(ViewModel, vm => vm.User, v => v.UserDetails.User);
            this.OneWayBind(ViewModel, vm => vm.User, v => v.UserAccountPanel.User);
            this.OneWayBind(ViewModel, vm => vm.User, v => v.UserProfilePanel.User);
            this.OneWayBind(ViewModel, vm => vm.User, v => v.UserGroups.User);

            this.OneWayBind(ViewModel, vm => vm.Computer, v => v.ComputerDetailsStackPanel.Visibility, x => x != null ? Visibility.Visible : Visibility.Collapsed);
            this.OneWayBind(ViewModel, vm => vm.Computer, v => v.ComputerDetails.Computer);
            this.OneWayBind(ViewModel, vm => vm.Computer, v => v.RemotePanel.Computer);
            this.OneWayBind(ViewModel, vm => vm.Computer, v => v.PingPanel.Computer);
            this.OneWayBind(ViewModel, vm => vm.Computer, v => v.ComputerGroups.Computer);

            this.OneWayBind(ViewModel, vm => vm.Group, v => v.GroupDetailsStackPanel.Visibility, x => x != null ? Visibility.Visible : Visibility.Collapsed);
            this.OneWayBind(ViewModel, vm => vm.Group, v => v.GroupDetails.Group);



            this.WhenActivated(d =>
            {
                d(this.BindCommand(ViewModel, vm => vm.NavigateBack, v => v.NavigateBackButton));
                d(this.BindCommand(ViewModel, vm => vm.NavigateForward, v => v.NavigateForwardButton));
                d(this.BindCommand(ViewModel, vm => vm.PasteAndFind, v => v.PasteAndFindButton));
                d(this.BindCommand(ViewModel, vm => vm.Find, v => v.FindButton));
                d(QueryStringTextBox.Events()
                    .KeyDown
                    .Where(x => x.Key == Key.Enter)
                    .Select(_ => Unit.Default)
                    .InvokeCommand(ViewModel, x => x.Find));
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
