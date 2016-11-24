using Microsoft.Win32;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Updater.Services;

namespace Updater.ViewModels
{
	public class MainWindowViewModel : ViewModelBase
	{
		public MainWindowViewModel()
		{
			_browseForSourceFile = ReactiveCommand.Create(BrowseForSourceFileImpl);
			_browseForSourceFile
				.Where(x => x.HasValue())
				.Subscribe(x => SourceFilePath = x);

			_browseForDestinationFolder = ReactiveCommand.Create(BrowseForDestinationFolderImpl);
			_browseForDestinationFolder
				.Where(x => x.HasValue())
				.Subscribe(x => DestinationFolderPath = x);

			_addDestinationFolder = ReactiveCommand.Create(
				() => _destinationFolders.Add(_destinationFolderPath),
				this.WhenAnyValue(x => x.DestinationFolderPath, x => x.HasValue()));
			_addDestinationFolder
				.Subscribe(_ => DestinationFolderPath = "");

			_removeDestinationFolder = ReactiveCommand.Create(
				() => { _destinationFolders.Remove(_selectedDestinationFolder as string); },
				this.WhenAnyValue(x => x.SelectedDestinationFolder).Select(x => x != null));

			_confirm = ReactiveCommand.CreateFromObservable(
				() => ConfirmImpl(_sourceFilePath, _destinationFolders),
				Observable.CombineLatest(
					this.WhenAnyValue(x => x.SourceFilePath, x => x.HasValue(1)),
					_destinationFolders.CountChanged.Select(x => x > 0),
					(sourceFilePath, destinationFolders) => sourceFilePath && destinationFolders));
			_confirm
				.ThrownExceptions
				.Subscribe(ex => MessageBox.Show(ex.Message, "An error occured", MessageBoxButtons.OK, MessageBoxIcon.Error));

			_destinationFoldersSortedView = _destinationFolders.CreateDerivedCollection(x => x, orderer: (x, y) => x.CompareTo(y));


			// State
			SourceFilePath = StateService.Current.SourceFilePath;
			_destinationFolders.AddRange(StateService.Current.DestinationFolders);

			this.WhenAnyValue(x => x.SourceFilePath)
				.Subscribe(x => StateService.Current.SourceFilePath = x);

			_destinationFolders.CountChanged
				.Select(_ => _destinationFolders)
				.Subscribe(x => StateService.Current.DestinationFolders = x);
		}



		public ReactiveCommand BrowseForSourceFile => _browseForSourceFile;

		public ReactiveCommand BrowseForDestinationFolder => _browseForDestinationFolder;

		public ReactiveCommand AddDestinationFolder => _addDestinationFolder;

		public ReactiveCommand RemoveDestinationFolder => _removeDestinationFolder;

		public ReactiveCommand Confirm => _confirm;

		public ReactiveList<string> DestinationFolders => _destinationFolders;

		public IReactiveDerivedList<string> DestinationFoldersSortedView => _destinationFoldersSortedView;

		public string SourceFilePath
		{
			get { return _sourceFilePath; }
			set { this.RaiseAndSetIfChanged(ref _sourceFilePath, value); }
		}

		public string DestinationFolderPath
		{
			get { return _destinationFolderPath; }
			set { this.RaiseAndSetIfChanged(ref _destinationFolderPath, value); }
		}

		public object SelectedDestinationFolder
		{
			get { return _selectedDestinationFolder; }
			set { this.RaiseAndSetIfChanged(ref _selectedDestinationFolder, value); }
		}



		private string BrowseForSourceFileImpl()
		{
			var dialog = new Microsoft.Win32.OpenFileDialog() { Filter = "Executables (*.exe)|*.exe" };
			if (dialog.ShowDialog() == true)
			{
				return dialog.FileName;
			}

			return "";
		}

		private string BrowseForDestinationFolderImpl()
		{
			var dialog = new FolderBrowserDialog();
			if (dialog.ShowDialog() == DialogResult.OK)
			{
				return dialog.SelectedPath;
			}

			return "";
		}

		private IObservable<bool> ConfirmImpl(string sourceFilePath, IEnumerable<string> destinationFolders) => Observable.Start(() =>
		{
			var file = new FileInfo(sourceFilePath);
			if (!file.Exists) throw new ArgumentException($"File \"{sourceFilePath}\" not found");

			foreach (var directoryPath in destinationFolders)
			{
				var directory = new DirectoryInfo(directoryPath);
				if (!directory.Exists) directory.Create();

				file.CopyTo(Path.Combine(directory.FullName, file.Name), true);
			}

			return true;
		});



		private readonly ReactiveCommand<Unit, string> _browseForSourceFile;
		private readonly ReactiveCommand<Unit, string> _browseForDestinationFolder;
		private readonly ReactiveCommand<Unit, Unit> _addDestinationFolder;
		private readonly ReactiveCommand<Unit, Unit> _removeDestinationFolder;
		private readonly ReactiveCommand<Unit, bool> _confirm;
		private readonly ReactiveList<string> _destinationFolders = new ReactiveList<string>();
		private readonly IReactiveDerivedList<string> _destinationFoldersSortedView;
		private string _sourceFilePath;
		private string _destinationFolderPath;
		private object _selectedDestinationFolder;
	}
}
