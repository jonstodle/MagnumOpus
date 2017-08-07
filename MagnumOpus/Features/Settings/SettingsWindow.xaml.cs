using ReactiveUI;
using System;
using System.Windows.Documents;
using System.Reactive.Linq;
using System.Windows.Navigation;
using System.Diagnostics;
using System.Reactive.Disposables;
using MagnumOpus.Dialog;

namespace MagnumOpus.Settings
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : WindowBase<SettingsWindowViewModel>
    {
        public SettingsWindow()
        {
            InitializeComponent();

            ViewModel = new SettingsWindowViewModel();

            this.WhenActivated(d =>
            {
                this.Bind(ViewModel, vm => vm.HistoryCountLimit, v => v.HistoryCountLimitTextBox.Text).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.OpenDuplicateWindows, v => v.OpenDuplicateWindowsCheckBox.IsChecked).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.DetailWindowTimeoutLength, v => v.DetailWindowTimeoutLengthTextBox.Text).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.UseEscapeToCloseDetailsWindows, v => v.UseEscapeToCloseDetailsWindowsCheckBox.IsChecked).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.Version, v => v.VersionTextBox.Text).DisposeWith(d);

                Observable.FromEventPattern<RequestNavigateEventArgs>(Icons8IconAttributionHyperlink, nameof(Hyperlink.RequestNavigate))
                    .Select(args => args.EventArgs.Uri.AbsoluteUri)
                    .Subscribe(uri => Process.Start(uri))
                    .DisposeWith(d);

                ViewModel
                    .Messages
                    .RegisterMessageHandler(ContainerGrid)
                    .DisposeWith(d);
            });
        }
    }
}
