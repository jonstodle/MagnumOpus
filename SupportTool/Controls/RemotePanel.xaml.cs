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

            this.WhenActivated(d =>
            {
                d(this.BindCommand(ViewModel, vm => vm.OpenLoggedOn, v => v.LoggedOnButton));
                d(this.BindCommand(ViewModel, vm => vm.OpenLoggedOnPlus, v => v.LoggedOnPlusButton));
				d(this.BindCommand(ViewModel, vm => vm.StartRemoteControl, v => v.RemoteControlButton));
				d(this.BindCommand(ViewModel, vm => vm.StartRemoteControlClassic, v => v.RemoteControlClassicButton));
				d(this.BindCommand(ViewModel, vm => vm.StartRemoteControl2012, v => v.RemoteControl2012Button));
				d(this.BindCommand(ViewModel, vm => vm.KillRemoteControl, v => v.KillRemoteControlButton));
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
