using Newtonsoft.Json;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Forms;
using Updater.Services;

namespace Updater.ViewModels
{
	public class MainWindowViewModel : ViewModelBase
	{
		public MainWindowViewModel()
		{
			_loadConfiguration = ReactiveCommand.CreateFromObservable(() =>
			{
				var filePath = BrowseForFile("Magnum Opus Updater file (*.mou)|*.mou");
				if (!filePath.HasValue()) return Observable.Return(Tuple.Create("", Enumerable.Empty<string>()));
				return LoadConfigurationImpl(filePath);
			});
			_loadConfiguration
				.Subscribe(x =>
				{
					SourceFilePath = x.Item1;
					using (_destinationFolders.SuppressChangeNotifications()) _destinationFolders.AddRange(x.Item2);
				});

			_saveConfiguration = ReactiveCommand.CreateFromObservable(() =>
			{
				var filePath = BrowseForSavePath("Magnum Opus Updater file (*.mou)|*.mou");
				if (!filePath.HasValue()) return Observable.Return(Unit.Default);
				return SaveConfigurationImpl(filePath, _sourceFilePath, _destinationFolders);
			});

			_browseForSourceFile = ReactiveCommand.Create(() => BrowseForFile("Executables (*.exe)|*.exe"));
			_browseForSourceFile
				.Where(x => x.HasValue())
				.Subscribe(x => SourceFilePath = x);

			_browseForDestinationFolder = ReactiveCommand.Create(BrowseForFolder);
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
				() => ConfirmImpl(_sourceFilePath, _destinationFolders, _killProcesses),
				Observable.CombineLatest(
					this.WhenAnyValue(x => x.SourceFilePath, x => x.HasValue(1)),
					_destinationFolders.CountChanged.Select(x => x > 0),
					(sourceFilePath, destinationFolders) => sourceFilePath && destinationFolders));
			_confirm
				.Subscribe(_ => MessageBox.Show("Files copied", "", MessageBoxButtons.OK, MessageBoxIcon.Information));

			Observable.Merge(
				_loadConfiguration.ThrownExceptions,
				_saveConfiguration.ThrownExceptions,
				_browseForSourceFile.ThrownExceptions,
				_browseForDestinationFolder.ThrownExceptions,
				_addDestinationFolder.ThrownExceptions,
				_removeDestinationFolder.ThrownExceptions,
				_confirm.ThrownExceptions)
				.Subscribe(ex => MessageBox.Show(ex.Message, "An error occured", MessageBoxButtons.OK, MessageBoxIcon.Error));

			_destinationFoldersSortedView = _destinationFolders.CreateDerivedCollection(x => x, orderer: (x, y) => x.CompareTo(y));


			// State
			SourceFilePath = StateService.Current.SourceFilePath;
			_destinationFolders.AddRange(StateService.Current.DestinationFolders);
			KillProcesses = StateService.Current.KillProcesses;

			this.WhenAnyValue(x => x.SourceFilePath)
				.Subscribe(x => StateService.Current.SourceFilePath = x);

			_destinationFolders.CountChanged
				.Select(_ => _destinationFolders)
				.Subscribe(x => StateService.Current.DestinationFolders = x);

			this.WhenAnyValue(x => x.KillProcesses)
				.Subscribe(x => StateService.Current.KillProcesses = x);
		}



		public ReactiveCommand LoadConfiguration => _loadConfiguration;

		public ReactiveCommand SaveConfiguration => _saveConfiguration;

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

		public bool KillProcesses
		{
			get { return _killProcesses; }
			set { this.RaiseAndSetIfChanged(ref _killProcesses, value); }
		}



		private string BrowseForSavePath(string filter)
		{
			var dialog = new Microsoft.Win32.SaveFileDialog { Filter = filter };
			if(dialog.ShowDialog() == true)
			{
				return dialog.FileName;
			}

			return "";
		}

		private string BrowseForFile(string filter)
		{
			var dialog = new Microsoft.Win32.OpenFileDialog() { Filter = filter };
			if (dialog.ShowDialog() == true)
			{
				return dialog.FileName;
			}

			return "";
		}

		private string BrowseForFolder()
		{
			var dialog = new FolderBrowserDialog();
			if (dialog.ShowDialog() == DialogResult.OK)
			{
				return dialog.SelectedPath;
			}

			return "";
		}

		private IObservable<Tuple<string, IEnumerable<string>>> LoadConfigurationImpl(string filePath) => Observable.Start(() =>
		{
			if (!filePath.HasValue()) throw new ArgumentException("No path provided", nameof(filePath));
			var json = File.ReadAllText(filePath);
			return JsonConvert.DeserializeObject<Tuple<string, IEnumerable<string>>>(json);
		});

		private IObservable<Unit> SaveConfigurationImpl(string savePath, string sourcePath, IEnumerable<string> destinationPaths) => Observable.Start(() =>
		{
			if (!savePath.HasValue()) throw new ArgumentException("No path provided", nameof(savePath));
			var json = JsonConvert.SerializeObject(Tuple.Create(sourcePath, destinationPaths));
			File.WriteAllText(savePath, json);
		});

		private IObservable<bool> ConfirmImpl(string sourceFilePath, IEnumerable<string> destinationFolders, bool killProcesses) => Observable.Start(() =>
		{
			var file = new FileInfo(sourceFilePath);
			if (!file.Exists) throw new ArgumentException($"File \"{sourceFilePath}\" not found");

			foreach (var directoryPath in destinationFolders)
			{
				if (killProcesses)
				{
					var taskkillPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "taskkill.exe");
					var arguments = "/im \"Magnum Opus.exe\" /f";

					if (directoryPath.StartsWith(@"\\")) arguments = $"/s {directoryPath.Split(new string[] { @"\" }, StringSplitOptions.RemoveEmptyEntries).First()} " + arguments;

					Process.Start(taskkillPath, arguments);
				}


				var directory = new DirectoryInfo(directoryPath);
				if (!directory.Exists) directory.Create();

				file.CopyTo(Path.Combine(directory.FullName, file.Name), true);
			}

			return true;
		});



		private readonly ReactiveCommand<Unit, Tuple<string, IEnumerable<string>>> _loadConfiguration;
		private readonly ReactiveCommand<Unit, Unit> _saveConfiguration;
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
		private bool _killProcesses;
	}
}
