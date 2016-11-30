using ReactiveUI;
using System.Windows;
using System.Windows.Controls;
using Updater.ViewModels;

namespace Updater
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window, IViewFor<MainWindowViewModel>
	{
		public MainWindow()
		{
			InitializeComponent();

			ViewModel = new MainWindowViewModel();

			this.Bind(ViewModel, vm => vm.SourceFilePath, v => v.SourceFileTextBox.Text);
			this.Bind(ViewModel, vm => vm.DestinationFolderPath, v => v.DestinationFolderTextBox.Text);
			this.OneWayBind(ViewModel, vm => vm.DestinationFoldersSortedView, v => v.DestinationFoldersListView.ItemsSource);
			this.Bind(ViewModel, vm => vm.SelectedDestinationFolder, v => v.DestinationFoldersListView.SelectedItem);
			this.Bind(ViewModel, vm => vm.KillProcesses, v => v.KillProcessesCheckBox.IsChecked);

			this.WhenActivated(d =>
			{
				d(this.BindCommand(ViewModel, vm => vm.LoadConfiguration, v => v.LoadConfigurationButton));
				d(this.BindCommand(ViewModel, vm => vm.SaveConfiguration, v => v.SaveConfigurationButton));
				d(this.BindCommand(ViewModel, vm => vm.BrowseForSourceFile, v => v.SourceFileBrowseButton));
				d(this.BindCommand(ViewModel, vm => vm.BrowseForDestinationFolder, v => v.DestinationFolderBrowseButton));
				d(this.BindCommand(ViewModel, vm => vm.AddDestinationFolder, v => v.AddDestinationFolderButton));
				d(this.BindCommand(ViewModel, vm => vm.RemoveDestinationFolder, v => v.DestinationFoldersListView, nameof(ListView.MouseDoubleClick)));
				d(this.BindCommand(ViewModel, vm => vm.Confirm, v => v.ConfirmButton));
			});
		}

		public MainWindowViewModel ViewModel
		{
			get { return (MainWindowViewModel)GetValue(ViewModelProperty); }
			set { SetValue(ViewModelProperty, value); }
		}

		public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(MainWindowViewModel), typeof(MainWindow), new PropertyMetadata(null));

		object IViewFor.ViewModel
		{
			get => ViewModel;
			set => ViewModel = value as MainWindowViewModel;
		}
	}
}
