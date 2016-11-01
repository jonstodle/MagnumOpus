using ReactiveUI;
using SupportTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
			this.Bind(ViewModel, vm => vm.HFName, v => v.HFNameTextBox.Text);
			this.OneWayBind(ViewModel, vm => vm.RemoteControl2012HFsView, v => v.RemoteControl2012HFsListView.ItemsSource);
			this.Bind(ViewModel, vm => vm.SelectedRemoteControl2012HF, v => v.RemoteControl2012HFsListView.SelectedItem);

			this.WhenActivated(d =>
			{
				d(HFNameTextBox.Events()
					.KeyDown
					.Where(x => x.Key == Key.Enter)
					.ToSignal()
					.InvokeCommand(ViewModel.AddHFName));
				d(this.BindCommand(ViewModel, vm => vm.AddHFName, v => v.AddHFNameButton));
				d(RemoteControl2012HFsListView.Events()
					.MouseDoubleClick
					.ToSignal()
					.InvokeCommand(ViewModel.RemoveHFName));
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
