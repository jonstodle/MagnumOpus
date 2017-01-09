using ReactiveUI;
using SupportTool.Models;
using SupportTool.ViewModels;
using System.Reactive;
using System.Windows;
using System.Windows.Controls;

namespace SupportTool.Controls
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

			this.Bind(ViewModel, vm => vm.IsShowingLoggedOnUsers, v => v.LoggedOnUsersToggleButton.IsChecked);
			this.OneWayBind(ViewModel, vm => vm.IsShowingLoggedOnUsers, v => v.LoggedOnUsersStackPanel.Visibility);
			this.OneWayBind(ViewModel, vm => vm.IsUacOn, v => v.ToggleUACButton.IsEnabled, x => x != null);
			this.OneWayBind(ViewModel, vm => vm.IsUacOn, v => v.ToggleUACButton.Content, x => x == null ? "Not available" : (bool)x ? "Disable UAC" : "Enable UAC");
			this.OneWayBind(ViewModel, vm => vm.LoggedOnUsers, v => v.LoggedOnUsersListView.ItemsSource);
			this.Bind(ViewModel, vm => vm.SelectedLoggedOnUser, v => v.LoggedOnUsersListView.SelectedItem);

			this.WhenActivated(d =>
            {
				d(this.BindCommand(ViewModel, vm => vm.OpenUser, v => v.LoggedOnUsersListView, nameof(ListView.MouseDoubleClick)));
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

		public ComputerObject Computer
        {
            get { return (ComputerObject)GetValue(ComputerProperty); }
            set { SetValue(ComputerProperty, value); }
        }

        public static readonly DependencyProperty ComputerProperty = DependencyProperty.Register(nameof(Computer), typeof(ComputerObject), typeof(RemotePanel), new PropertyMetadata(null, (d,e)=> (d as RemotePanel).ViewModel.Computer = e.NewValue as ComputerObject));

        public RemotePanelViewModel ViewModel
        {
            get { return (RemotePanelViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(RemotePanelViewModel), typeof(RemotePanel), new PropertyMetadata(null));

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = value as RemotePanelViewModel; }
        }
    }
}
