using ReactiveUI;
using MagnumOpus.Models;
using MagnumOpus.ViewModels;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Reactive.Disposables;

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
                this.Bind(ViewModel, vm => vm.Computer, v => v.Computer).DisposeWith(d);

                this.Bind(ViewModel, vm => vm.IsShowingLoggedOnUsers, v => v.LoggedOnUsersToggleButton.IsChecked).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.IsShowingLoggedOnUsers, v => v.LoggedOnUsersStackPanel.Visibility).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.IsUacOn, v => v.ToggleUACButton.IsEnabled, isUacOn => isUacOn != null).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.IsUacOn, v => v.ToggleUACButton.Content, isUacOn => isUacOn ?? true ? "Disable UAC" : "Enable UAC").DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.LoggedOnUsers, v => v.LoggedOnUsersListView.ItemsSource).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.SelectedLoggedOnUser, v => v.LoggedOnUsersListView.SelectedItem).DisposeWith(d);

                _loggedOnUsersListViewItemDoubleClick.ToEventCommandSignal().InvokeCommand(ViewModel.OpenUser).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.OpenUser, v => v.OpenUserMenuItem).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.CopyUserName, v => v.CopyUsernameMenuItem).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.LogOffUser, v => v.LogOffUserMenuItem).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.StartRemoteControl, v => v.RemoteControlButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.StartRemoteControlClassic, v => v.RemoteControlClassicButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.StartRemoteControl2012, v => v.RemoteControl2012Button).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.KillRemoteTools, v => v.KillRemoteToolsButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.ToggleUac, v => v.ToggleUACButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.StartRemoteAssistance, v => v.RemoteAssistanceButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.StartRdp, v => v.RdpButton).DisposeWith(d);
            });
        }

        public Interaction<MessageInfo, int> Messages => ViewModel.Messages;

        public ComputerObject Computer { get => (ComputerObject)GetValue(ComputerProperty); set => SetValue(ComputerProperty, value); }
        public static readonly DependencyProperty ComputerProperty = DependencyProperty.Register(nameof(Computer), typeof(ComputerObject), typeof(RemotePanel), new PropertyMetadata(null));

        public RemotePanelViewModel ViewModel { get => (RemotePanelViewModel)GetValue(ViewModelProperty); set => SetValue(ViewModelProperty, value); }
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(RemotePanelViewModel), typeof(RemotePanel), new PropertyMetadata(null));

        object IViewFor.ViewModel { get => ViewModel; set => ViewModel = value as RemotePanelViewModel; }

        private Subject<MouseButtonEventArgs> _loggedOnUsersListViewItemDoubleClick = new Subject<MouseButtonEventArgs>();
        private void LoggedOnUsersListViewItem_DoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e) => _loggedOnUsersListViewItemDoubleClick.OnNext(e);
    }
}
