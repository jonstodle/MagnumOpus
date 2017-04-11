using ReactiveUI;
using MagnumOpus.Models;
using MagnumOpus.ViewModels;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MagnumOpus.Controls
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

            this.WhenActivated(d =>
            {
                d(this.Bind(ViewModel, vm => vm.HostName, v => v.HostName));

                d(this.Bind(ViewModel, vm => vm.IsPinging, v => v.PingToggleButton.IsChecked));
                d(this.OneWayBind(ViewModel, vm => vm.IsPinging, v => v.PingToggleButton.Content, x => !x ? "Start" : "Stop"));
                d(this.OneWayBind(ViewModel, vm => vm.MostRecentPingResult, v => v.PingResultTextBlock.Text));
                d(this.OneWayBind(ViewModel, vm => vm.IsPinging, v => v.PingResultDetailsToggleButton.IsEnabled));
                d(this.Bind(ViewModel, vm => vm.IsShowingPingResultDetails, v => v.PingResultDetailsToggleButton.IsChecked));
                d(this.OneWayBind(ViewModel, vm => vm.IsShowingPingResultDetails, v => v.PingResultDetailsStackPanel.Visibility));
                d(this.OneWayBind(ViewModel, vm => vm.PingResults, v => v.PingResultDetailsListView.ItemsSource));

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

        public Interaction<MessageInfo, int> Messages => ViewModel.Messages;

        public string HostName { get => (string)GetValue(HostNameProperty); set => SetValue(HostNameProperty, value); }
        public static readonly DependencyProperty HostNameProperty = DependencyProperty.Register(nameof(HostName), typeof(string), typeof(PingPanel), new PropertyMetadata(null));

        public PingPanelViewModel ViewModel { get => (PingPanelViewModel)GetValue(ViewModelProperty); set => SetValue(ViewModelProperty, value); }
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(PingPanelViewModel), typeof(PingPanel), new PropertyMetadata(null));

        object IViewFor.ViewModel { get => ViewModel; set => ViewModel = value as PingPanelViewModel; }
    }
}
