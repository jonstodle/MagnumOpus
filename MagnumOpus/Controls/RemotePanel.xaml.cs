using ReactiveUI;
using MagnumOpus.Models;
using MagnumOpus.ViewModels;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MagnumOpus.Controls
{
    /// <summary>
    /// Interaction logic for RemotePanel.xaml
    /// </summary>
    public partial class RemotePanel : UserControl, IViewFor<RemotePanelViewModel>
    {
        public RemotePanel()
        {
            InitializeComponent();

            ViewModel = new RemotePanelViewModel();

            this.WhenActivated(d =>
            {
                d(this.Bind(ViewModel, vm => vm.Computer, v => v.Computer));

                d(this.Bind(ViewModel, vm => vm.IsShowingLoggedOnUsers, v => v.LoggedOnUsersToggleButton.IsChecked));
                d(this.OneWayBind(ViewModel, vm => vm.IsShowingLoggedOnUsers, v => v.LoggedOnUsersStackPanel.Visibility));
                d(this.OneWayBind(ViewModel, vm => vm.IsUacOn, v => v.ToggleUACButton.IsEnabled, x => x != null));
                d(this.OneWayBind(ViewModel, vm => vm.IsUacOn, v => v.ToggleUACButton.Content, x => x == null ? "Not available" : (bool)x ? "Disable UAC" : "Enable UAC"));
                d(this.OneWayBind(ViewModel, vm => vm.LoggedOnUsers, v => v.LoggedOnUsersListView.ItemsSource));
                d(this.Bind(ViewModel, vm => vm.SelectedLoggedOnUser, v => v.LoggedOnUsersListView.SelectedItem));

                d(_loggedOnUsersListViewItemDoubleClick.ToEventCommandSignal().InvokeCommand(ViewModel.OpenUser));
                d(this.BindCommand(ViewModel, vm => vm.OpenUser, v => v.OpenUserMenuItem));
                d(this.BindCommand(ViewModel, vm => vm.CopyUserName, v => v.CopyUsernameMenuItem));
                d(this.BindCommand(ViewModel, vm => vm.LogOffUser, v => v.LogOffUserMenuItem));
                d(this.BindCommand(ViewModel, vm => vm.OpenLoggedOnUserDetails, v => v.OpenLoggedOnUsersDetailsButton));
                d(this.BindCommand(ViewModel, vm => vm.StartRemoteControl, v => v.RemoteControlButton));
                d(this.BindCommand(ViewModel, vm => vm.StartRemoteControlClassic, v => v.RemoteControlClassicButton));
                d(this.BindCommand(ViewModel, vm => vm.StartRemoteControl2012, v => v.RemoteControl2012Button));
                d(this.BindCommand(ViewModel, vm => vm.KillRemoteTools, v => v.KillRemoteToolsButton));
                d(this.BindCommand(ViewModel, vm => vm.ToggleUac, v => v.ToggleUACButton));
                d(this.BindCommand(ViewModel, vm => vm.StartRemoteAssistance, v => v.RemoteAssistanceButton));
                d(this.BindCommand(ViewModel, vm => vm.StartRdp, v => v.RdpButton));
            });
        }

        public Interaction<MessageInfo, Unit> InfoMessages => ViewModel.InfoMessages;

        public Interaction<MessageInfo, Unit> ErrorMessages => ViewModel.ErrorMessages;

        public ComputerObject Computer { get => (ComputerObject)GetValue(ComputerProperty); set => SetValue(ComputerProperty, value); }
        public static readonly DependencyProperty ComputerProperty = DependencyProperty.Register(nameof(Computer), typeof(ComputerObject), typeof(RemotePanel), new PropertyMetadata(null));

        public RemotePanelViewModel ViewModel { get => (RemotePanelViewModel)GetValue(ViewModelProperty); set => SetValue(ViewModelProperty, value); }
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(RemotePanelViewModel), typeof(RemotePanel), new PropertyMetadata(null));

        object IViewFor.ViewModel { get => ViewModel; set => ViewModel = value as RemotePanelViewModel; }

        private Subject<MouseButtonEventArgs> _loggedOnUsersListViewItemDoubleClick = new Subject<MouseButtonEventArgs>();
        private void LoggedOnUsersListViewItem_DoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e) => _loggedOnUsersListViewItemDoubleClick.OnNext(e);
    }
}
