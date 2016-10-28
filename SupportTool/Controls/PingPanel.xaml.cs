using ReactiveUI;
using SupportTool.Models;
using SupportTool.ViewModels;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SupportTool.Controls
{
	/// <summary>
	/// Interaction logic for PingPanel.xaml
	/// </summary>
	public partial class PingPanel : UserControl, IViewFor<PingPanelViewModel>
    {
        public PingPanel()
        {
            InitializeComponent();

			ViewModel = new PingPanelViewModel();

			this.Bind(ViewModel, vm => vm.IsPinging, v => v.PingToggleButton.IsChecked);
			this.OneWayBind(ViewModel, vm => vm.IsPinging, v => v.PingToggleButton.Content, x => !x ? "Start" : "Stop");
			this.OneWayBind(ViewModel, vm => vm.MostRecentPingResult, v => v.PingResultTextBlock.Text);
            this.OneWayBind(ViewModel, vm => vm.IsPinging, v => v.PingResultDetailsToggleButton.IsEnabled);
            this.Bind(ViewModel, vm => vm.IsShowingPingResultDetails, v => v.PingResultDetailsToggleButton.IsChecked);
            this.OneWayBind(ViewModel, vm => vm.IsShowingPingResultDetails, v => v.PingResultDetailsStackPanel.Visibility);
            this.OneWayBind(ViewModel, vm => vm.PingResults, v => v.PingResultDetailsListView.ItemsSource);

            this.WhenActivated(d =>
            {
				d(ViewModel
					.WhenAnyValue(x => x.IsPinging)
					.Where(x => x)
					.Select(_ => Unit.Default)
					.ObserveOnDispatcher()
					.InvokeCommand(ViewModel, x => x.StartPing));
				d(ViewModel
					.WhenAnyValue(x => x.IsPinging)
					.Where(x => !x)
					.Select(_ => Unit.Default)
					.ObserveOnDispatcher()
					.InvokeCommand(ViewModel, x => x.StopPing));
				d(ViewModel
					.WhenAnyValue(x => x.HostName)
					.Where(x => x == null)
					.Select(_ => Unit.Default)
					.ObserveOnDispatcher()
					.InvokeCommand(ViewModel, x => x.StopPing));
            });
        }

		public string HostName
		{
			get { return (string)GetValue(HostNameProperty); }
			set { SetValue(HostNameProperty, value); }
		}

		public static readonly DependencyProperty HostNameProperty = DependencyProperty.Register(nameof(HostName), typeof(string), typeof(PingPanel), new PropertyMetadata(null, (d,e) => (d as PingPanel).ViewModel.HostName = e.NewValue as string));

		public PingPanelViewModel ViewModel
        {
            get { return (PingPanelViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(PingPanelViewModel), typeof(PingPanel), new PropertyMetadata(null));

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = value as PingPanelViewModel; }
        }
    }
}
