using ReactiveUI;
using MagnumOpus.Models;
using MagnumOpus.ViewModels;
using System;
using System.Windows;
using System.Windows.Documents;
using System.Reactive.Linq;
using System.Windows.Navigation;
using System.Diagnostics;

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
                d(this.Bind(ViewModel, vm => vm.OpenDuplicateWindows, v => v.OpenDuplicateWindowsCheckBox.IsChecked));
                d(this.Bind(ViewModel, vm => vm.DetailWindowTimeoutLength, v => v.DetailWindowTimeoutLengthTextBox.Text));
                d(this.Bind(ViewModel, vm => vm.UseEscapeToCloseDetailsWindows, v => v.UseEscapeToCloseDetailsWindowsCheckBox.IsChecked));
                d(this.Bind(ViewModel, vm => vm.RemoteControlClassicPath, v => v.RemoteControlClassicPathTextBox.Text));
                d(this.Bind(ViewModel, vm => vm.RemoteControl2012Path, v => v.RemoteControl2012PathTextBox.Text));
                d(this.OneWayBind(ViewModel, vm => vm.Version, v => v.VersionTextBlock.Text));

                d(Observable.Merge(
                    Observable.FromEventPattern<RequestNavigateEventArgs>(SupportIconAttributionHyperlink, nameof(Hyperlink.RequestNavigate)),
                    Observable.FromEventPattern<RequestNavigateEventArgs>(InfoIconAttributionHyperlink, nameof(Hyperlink.RequestNavigate)),
                    Observable.FromEventPattern<RequestNavigateEventArgs>(QuestionIconAttributionHyperlink, nameof(Hyperlink.RequestNavigate)),
                    Observable.FromEventPattern<RequestNavigateEventArgs>(SuccessIconAttributionHyperlink, nameof(Hyperlink.RequestNavigate)),
                    Observable.FromEventPattern<RequestNavigateEventArgs>(WarningIconAttributionHyperlink, nameof(Hyperlink.RequestNavigate)),
                    Observable.FromEventPattern<RequestNavigateEventArgs>(ErrorIconAttributionHyperlink, nameof(Hyperlink.RequestNavigate)))
                    .Select(args => args.EventArgs.Uri.AbsoluteUri)
                    .Subscribe(uri => Process.Start(uri)));

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
