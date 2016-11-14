using ReactiveUI;
using SupportTool.Models;
using SupportTool.ViewModels;
using System.Collections.Generic;
using System.Reactive;
using System.Windows;

namespace SupportTool.Views
{
	/// <summary>
	/// Interaction logic for IPAddressWindow.xaml
	/// </summary>
	public partial class IPAddressWindow : DetailsWindow, IViewFor<IPAddressWindowViewModel>
	{
		public IPAddressWindow()
		{
			InitializeComponent();

			ViewModel = new IPAddressWindowViewModel();

			this.OneWayBind(ViewModel, vm => vm.IPAddress, v => v.Title, x => x ?? "");
			this.OneWayBind(ViewModel, vm => vm.IPAddress, v => v.IPAddressPanel.IPAddress);
			this.OneWayBind(ViewModel, vm => vm.IPAddress, v => v.PingPanel.HostName);

			this.WhenActivated(d =>
			{
				d(IPAddressPanel
					.PromptMessages
					.RegisterPromptHandler(ContainerGrid));
				d(new List<Interaction<MessageInfo, Unit>>
				{
					IPAddressPanel.InfoMessages,
					PingPanel.InfoMessages
				}.RegisterInfoHandler(ContainerGrid));
				d(new List<Interaction<MessageInfo, Unit>>
				{
					IPAddressPanel.ErrorMessages,
					PingPanel.ErrorMessages
				}.RegisterErrorHandler(ContainerGrid));
			});
		}

		public IPAddressWindowViewModel ViewModel
		{
			get { return (IPAddressWindowViewModel)GetValue(ViewModelProperty); }
			set { SetValue(ViewModelProperty, value); }
		}

		public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(IPAddressWindowViewModel), typeof(IPAddressWindow), new PropertyMetadata(null));

		object IViewFor.ViewModel
		{
			get { return ViewModel; }
			set { ViewModel = value as IPAddressWindowViewModel; }
		}
	}
}
