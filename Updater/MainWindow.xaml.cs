﻿using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
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

			this.WhenActivated(d =>
			{
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
