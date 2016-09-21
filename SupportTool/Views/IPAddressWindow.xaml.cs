using ReactiveUI;
using SupportTool.ViewModels;
using System.Windows;

namespace SupportTool.Views
{
	/// <summary>
	/// Interaction logic for IPAddressWindow.xaml
	/// </summary>
	public partial class IPAddressWindow : Window, IViewFor<IPAddressWindowViewModel>
	{
		public IPAddressWindow()
		{
			InitializeComponent();

			ViewModel = new IPAddressWindowViewModel();

			this.OneWayBind(ViewModel, vm => vm.IPAddress, v => v.Title, x => x ?? "");
			this.OneWayBind(ViewModel, vm => vm.IPAddress, v => v.IPAddressPanel.IPAddress);
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
