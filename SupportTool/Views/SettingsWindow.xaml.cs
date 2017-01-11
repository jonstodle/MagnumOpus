using ReactiveUI;
using SupportTool.Models;
using SupportTool.ViewModels;
using System.Windows;

namespace SupportTool.Views
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

			this.Bind(ViewModel, vm => vm.HistoryCountLimit, v => v.HistoryCountLimitTextBox.Text);
			this.Bind(ViewModel, vm => vm.DetailWindowTimeoutLength, v => v.DetailWindowTimeoutLengthTextBox.Text);
            this.Bind(ViewModel, vm => vm.UseEscapeToCloseDetailsWindows, v => v.UseEscapeToCloseDetailsWindowsCheckBox.IsChecked);
            this.Bind(ViewModel, vm => vm.RemoteControlClassicPath, v => v.RemoteControlClassicPathTextBox.Text);
			this.Bind(ViewModel, vm => vm.RemoteControl2012Path, v => v.RemoteControl2012PathTextBox.Text);

			this.WhenActivated(d =>
			{
				d(ViewModel
					.InfoMessages
					.RegisterInfoHandler(ContainerGrid));
				d(ViewModel
					.ErrorMessages
					.RegisterErrorHandler(ContainerGrid));
			});
		}

		public SettingsWindowViewModel ViewModel
		{
			get { return (SettingsWindowViewModel)GetValue(ViewModelProperty); }
			set { SetValue(ViewModelProperty, value); }
		}

		public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(SettingsWindowViewModel), typeof(SettingsWindow), new PropertyMetadata(null));

		object IViewFor.ViewModel
		{
			get { return ViewModel; }
			set { ViewModel = value as SettingsWindowViewModel; }
		}
	}
}
