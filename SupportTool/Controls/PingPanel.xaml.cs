using ReactiveUI;
using SupportTool.Models;
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
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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

            this.OneWayBind(ViewModel, vm => vm.IsPinging, v => v.StartPingButton.Visibility, x => !x ? Visibility.Visible : Visibility.Collapsed);
            this.OneWayBind(ViewModel, vm => vm.IsPinging, v => v.StopPingButton.Visibility, x => x ? Visibility.Visible : Visibility.Collapsed);
            this.OneWayBind(ViewModel, vm => vm.MostRecentPingResult, v => v.PingResultTextBlock.Text);
            this.OneWayBind(ViewModel, vm => vm.IsPinging, v => v.PingResultDetailsToggleButton.IsEnabled);
            this.Bind(ViewModel, vm => vm.IsShowingPingResultDetails, v => v.PingResultDetailsToggleButton.IsChecked);
            this.OneWayBind(ViewModel, vm => vm.IsShowingPingResultDetails, v => v.PingResultDetailsStackPanel.Visibility);
            this.OneWayBind(ViewModel, vm => vm.PingResults, v => v.PingResultDetailsListView.ItemsSource);

            this.WhenActivated(d =>
            {
                d(this.BindCommand(ViewModel, vm => vm.StartPing, v => v.StartPingButton));
                d(this.BindCommand(ViewModel, vm => vm.StopPing, v => v.StopPingButton));
            });
        }

        public IObservable<Message> Messages => ViewModel.Messages;

        public ComputerObject Computer
        {
            get { return (ComputerObject)GetValue(ComputerProperty); }
            set { SetValue(ComputerProperty, value); }
        }

        public static readonly DependencyProperty ComputerProperty = DependencyProperty.Register(nameof(Computer), typeof(ComputerObject), typeof(PingPanel), new PropertyMetadata(null, (d,e) => (d as PingPanel).ViewModel.Computer = e.NewValue as ComputerObject));

        public PingPanelViewModel ViewModel
        {
            get { return (PingPanelViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(PingPanelViewModel), typeof(PingPanel), new PropertyMetadata(new PingPanelViewModel()));

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = value as PingPanelViewModel; }
        }
    }
}
