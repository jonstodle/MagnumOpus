using ReactiveUI;
using MagnumOpus.Models;
using MagnumOpus.ViewModels;
using System.Windows;

namespace MagnumOpus.Views
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window, IViewFor<SettingsWindowViewModel>
    {
        public SettingsWindow()
        {
            InitializeComponent();

            ViewModel = new SettingsWindowViewModel();

            this.WhenActivated(d =>
            {
                d(this.Bind(ViewModel, vm => vm.HistoryCountLimit, v => v.HistoryCountLimitTextBox.Text));
                d(this.Bind(ViewModel, vm => vm.DetailWindowTimeoutLength, v => v.DetailWindowTimeoutLengthTextBox.Text));
                d(this.Bind(ViewModel, vm => vm.UseEscapeToCloseDetailsWindows, v => v.UseEscapeToCloseDetailsWindowsCheckBox.IsChecked));
                d(this.Bind(ViewModel, vm => vm.RemoteControlClassicPath, v => v.RemoteControlClassicPathTextBox.Text));
                d(this.Bind(ViewModel, vm => vm.RemoteControl2012Path, v => v.RemoteControl2012PathTextBox.Text));
                d(this.OneWayBind(ViewModel, vm => vm.Version, v => v.VersionTextBlock.Text));

                d(ViewModel
                    .InfoMessages
                    .RegisterInfoHandler(ContainerGrid));
                d(ViewModel
                    .ErrorMessages
                    .RegisterErrorHandler(ContainerGrid));
            });
        }

        public SettingsWindowViewModel ViewModel { get => (SettingsWindowViewModel)GetValue(ViewModelProperty); set => SetValue(ViewModelProperty, value); }
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(SettingsWindowViewModel), typeof(SettingsWindow), new PropertyMetadata(null));

        object IViewFor.ViewModel { get => ViewModel; set => ViewModel = value as SettingsWindowViewModel; }
    }
}
