using Microsoft.Win32;
using ReactiveUI;
using SupportTool.Models;
using SupportTool.Services.DialogServices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Reactive;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace SupportTool.ViewModels
{
	public class ProfilePanelViewModel : ReactiveObject
	{
		private readonly ReactiveCommand<Unit, string> _resetGlobalProfile;
		private readonly ReactiveCommand<Unit, string> _resetLocalProfile;
		private readonly ReactiveCommand<Unit, Unit> _openGlobalProfileDirectory;
		private readonly ReactiveCommand<Unit, Unit> _openLocalProfileDirectory;
		private readonly ReactiveCommand<Unit, Tuple<DirectoryInfo, IEnumerable<DirectoryInfo>>> _searchForProfiles;
		private readonly ReactiveCommand<Unit, Unit> _restoreProfile;
		private readonly ReactiveList<string> _resetMessages;
		private readonly ReactiveList<DirectoryInfo> _profiles;
		private UserObject _user;
		private bool _isShowingResetProfile;
		private bool _isShowingRestoreProfile;
		private string _computerName;
		private bool _shouldRestoreDesktopItems;
		private bool _shouldRestoreInternetExplorerFavorites;
		private bool _shouldRestoreOutlookSignatures;
		private bool _shouldRestoreWindowsExplorerFavorites;
		private bool _shouldRestoreStickyNotes;
		private int _selectedProfileIndex;
		private DirectoryInfo _globalProfileDirectory;
		private DirectoryInfo _localProfileDirectory;
		private DirectoryInfo _newProfileDirectory;



		public ProfilePanelViewModel()
		{
			_resetMessages = new ReactiveList<string>();
			_profiles = new ReactiveList<DirectoryInfo>();
			_shouldRestoreDesktopItems = true;
			_shouldRestoreInternetExplorerFavorites = true;
			_shouldRestoreOutlookSignatures = true;
			_shouldRestoreWindowsExplorerFavorites = true;
			_shouldRestoreStickyNotes = true;

			_resetGlobalProfile = ReactiveCommand.CreateFromObservable(() => ResetGlobalProfileImpl(_user));
			_resetGlobalProfile
				.Subscribe(x => _resetMessages.Insert(0, x));
			_resetGlobalProfile
				.ThrownExceptions
				.Subscribe(ex =>
				{
					_resetMessages.Insert(0, CreateLogString("Could not reset global profile"));
					DialogService.ShowError(ex.Message);
				});

			_resetLocalProfile = ReactiveCommand.CreateFromObservable(
				() => ResetLocalProfileImpl(_user, _computerName),
				this.WhenAnyValue(x => x.ComputerName, x => x.HasValue(6)));
			_resetLocalProfile
				.Subscribe(x => _resetMessages.Insert(0, x));
			_resetLocalProfile
				.ThrownExceptions
				.Subscribe(ex =>
				{
					_resetMessages.Insert(0, CreateLogString("Could not reset local profile"));
					DialogService.ShowError(ex.Message);
				});

			_openGlobalProfileDirectory = ReactiveCommand.Create(
				() =>
				{
					var gpdPath = GetParentDirectory(_user.ProfilePath);
					if (gpdPath == null || !gpdPath.Exists) throw new Exception($"Could not find global profile folder");
					Process.Start(gpdPath.FullName);
				});
			_openGlobalProfileDirectory
				.ThrownExceptions
				.Subscribe(ex => DialogService.ShowError(ex.Message, "Could not open directory"));

			_openLocalProfileDirectory = ReactiveCommand.Create(
				() =>
				{
					var lpd = GetProfileDirectory(ComputerName);
					if (lpd == null || !lpd.Exists) throw new Exception($"Could not find local profile folder");
					Process.Start(lpd.FullName);
				},
				this.WhenAnyValue(x => x.ComputerName, x => x.HasValue(6)));
			_openLocalProfileDirectory
				.ThrownExceptions
				.Subscribe(ex => DialogService.ShowError(ex.Message, "Could not open directory"));

			_searchForProfiles = ReactiveCommand.CreateFromObservable(() =>
			{
				_profiles.Clear();
				return SearchForProfilesImpl(_user, _computerName);
			},
			this.WhenAnyValue(x => x.ComputerName, x => x.HasValue()));
			_searchForProfiles
				.Subscribe(x =>
				{
					NewProfileDirectory = x.Item1;
					using (_profiles.SuppressChangeNotifications()) _profiles.AddRange(x.Item2);
				});
			_searchForProfiles
				.ThrownExceptions
				.Subscribe(ex => DialogService.ShowError(ex.Message));

			_restoreProfile = ReactiveCommand.CreateFromObservable(
				() => RestoreProfileImpl(NewProfileDirectory, _profiles[SelectedProfileIndex]),
				this.WhenAnyValue(x => x.NewProfileDirectory, y => y.SelectedProfileIndex, (x, y) => x != null && y >= 0));
			_restoreProfile
				.Subscribe(_ => DialogService.ShowInfo("Profile restored", "Success"));
			_restoreProfile
				.ThrownExceptions
				.Subscribe(ex => DialogService.ShowError(ex.Message, "Could not restore profile"));

			this
				.WhenAnyValue(x => x.IsShowingResetProfile)
				.Where(x => x)
				.Subscribe(_ => IsShowingRestoreProfile = false);

			this
				.WhenAnyValue(x => x.IsShowingRestoreProfile)
				.Where(x => x)
				.Subscribe(_ => IsShowingResetProfile = false);
		}



		public ReactiveCommand ResetGlobalProfile => _resetGlobalProfile;

		public ReactiveCommand ResetLocalProfile => _resetLocalProfile;

		public ReactiveCommand OpenGlobalProfileDirectory => _openGlobalProfileDirectory;

		public ReactiveCommand OpenLocalProfileDirectory => _openLocalProfileDirectory;

		public ReactiveCommand SearchForProfiles => _searchForProfiles;

		public ReactiveCommand RestoreProfile => _restoreProfile;

		public ReactiveList<string> ResetMessages => _resetMessages;

		public ReactiveList<DirectoryInfo> Profiles => _profiles;

		public UserObject User
		{
			get { return _user; }
			set { this.RaiseAndSetIfChanged(ref _user, value); }
		}

		public bool IsShowingResetProfile
		{
			get { return _isShowingResetProfile; }
			set { this.RaiseAndSetIfChanged(ref _isShowingResetProfile, value); }
		}

		public bool IsShowingRestoreProfile
		{
			get { return _isShowingRestoreProfile; }
			set { this.RaiseAndSetIfChanged(ref _isShowingRestoreProfile, value); }
		}

		public string ComputerName
		{
			get { return _computerName; }
			set { this.RaiseAndSetIfChanged(ref _computerName, value); }
		}

		public bool ShouldRestoreDesktopItems
		{
			get { return _shouldRestoreDesktopItems; }
			set { this.RaiseAndSetIfChanged(ref _shouldRestoreDesktopItems, value); }
		}

		public bool ShouldRestoreInternetExplorerFavorites
		{
			get { return _shouldRestoreInternetExplorerFavorites; }
			set { this.RaiseAndSetIfChanged(ref _shouldRestoreInternetExplorerFavorites, value); }
		}

		public bool ShouldRestoreOutlookSignatures
		{
			get { return _shouldRestoreOutlookSignatures; }
			set { this.RaiseAndSetIfChanged(ref _shouldRestoreOutlookSignatures, value); }
		}

		public bool ShouldRestoreWindowsExplorerFavorites
		{
			get { return _shouldRestoreWindowsExplorerFavorites; }
			set { this.RaiseAndSetIfChanged(ref _shouldRestoreWindowsExplorerFavorites, value); }
		}

		public bool ShouldRestoreStickyNotes
		{
			get { return _shouldRestoreStickyNotes; }
			set { this.RaiseAndSetIfChanged(ref _shouldRestoreStickyNotes, value); }
		}

		public int SelectedProfileIndex
		{
			get { return _selectedProfileIndex; }
			set { this.RaiseAndSetIfChanged(ref _selectedProfileIndex, value); }
		}

		public DirectoryInfo GlobalProfileDirectory
		{
			get { return _globalProfileDirectory; }
			set { this.RaiseAndSetIfChanged(ref _globalProfileDirectory, value); }
		}

		public DirectoryInfo LocalProfileDirectory
		{
			get { return _localProfileDirectory; }
			set { this.RaiseAndSetIfChanged(ref _localProfileDirectory, value); }
		}

		public DirectoryInfo NewProfileDirectory
		{
			get { return _newProfileDirectory; }
			set { this.RaiseAndSetIfChanged(ref _newProfileDirectory, value); }
		}



		private IObservable<string> ResetGlobalProfileImpl(UserObject usr) => Observable.Create<string>(observer =>
		{
			observer.OnNext(CreateLogString("Resetting global profile"));

			Thread.Sleep(3000);

			var globalProfileDirecotry = GetParentDirectory(usr.ProfilePath);
			if (globalProfileDirecotry == null) throw new Exception("Could not get profile path");

			foreach (var dir in globalProfileDirecotry.GetDirectories($"{usr.Principal.SamAccountName}*"))
			{
				BangRenameDirectory(dir, usr.Principal.SamAccountName);
				observer.OnNext(CreateLogString($"Renamed folder {dir.FullName}"));
			}

			observer.OnNext(CreateLogString("Successfully reset global profile"));
			observer.OnCompleted();
			return () => { };
		}).SubscribeOn(RxApp.TaskpoolScheduler);

		private IObservable<string> ResetLocalProfileImpl(UserObject usr, string cpr) => Observable.Create<string>(observer =>
		{
			if (PingNameOrAddressAsync(cpr) < 0) throw new Exception($"Could not connect to {cpr}");
			observer.OnNext(CreateLogString("Computer found"));

			if (GetLoggedInUsers(cpr).Select(x => x.ToLowerInvariant()).Contains(usr.Principal.SamAccountName)) throw new Exception("User is logged in");
			observer.OnNext(CreateLogString("Confirmed user is not logged in"));

			observer.OnNext(CreateLogString("Resetting local profile"));

			var profileDir = GetProfileDirectory(cpr);
			foreach (var dir in profileDir.GetDirectories($"{usr.Principal.SamAccountName}*"))
			{
				BangRenameDirectory(dir, usr.Principal.SamAccountName);
				observer.OnNext(CreateLogString($"Renamed folder {dir.FullName}"));
			}

			var scope = new ManagementScope(@"\\PC26678\root\cimv2");
			scope.Connect();
			using (var searcher = new ManagementObjectSearcher(scope, new ObjectQuery("SELECT * FROM Win32_UserProfile")))
			{
				foreach (ManagementObject userObject in searcher.Get())
				{
					if (userObject.Properties["LocalPath"].Value.ToString() == Path.Combine(@"C:\", "Users", usr.Principal.SamAccountName))
					{
						userObject.Delete();
						observer.OnNext(CreateLogString("Deleted local profile references"));
					}
				} 
			}

			observer.OnNext(CreateLogString("Successfully reset local profile"));
			observer.OnCompleted();
			return () => { };
		}).SubscribeOn(RxApp.TaskpoolScheduler);

		private IObservable<Tuple<DirectoryInfo, IEnumerable<DirectoryInfo>>> SearchForProfilesImpl(UserObject usr, string cpr) => Observable.Start(() =>
		{
			if (PingNameOrAddressAsync(cpr) < 0) throw new Exception($"Could not connect to {cpr}");

			var profilesDirectory = new DirectoryInfo($@"\\{cpr}\C$\Users");
			var profileDirectories = profilesDirectory.GetDirectories($"*{usr.Principal.SamAccountName}*").ToList();

			var profileDir = profileDirectories.FirstOrDefault(x => x.Name.ToLowerInvariant() == usr.Principal.SamAccountName.ToLowerInvariant());
			if (profileDir == null) throw new Exception("No local profile folder");
			profileDirectories.Remove(profileDir);

			return Tuple.Create(profileDir, profileDirectories.AsEnumerable());
		});

		private IObservable<Unit> RestoreProfileImpl(DirectoryInfo newProfileDir, DirectoryInfo oldProfileDir) => Observable.Start(() =>
		{
			if (ShouldRestoreDesktopItems) { CopyDirectoryContents(oldProfileDir.FullName, newProfileDir.FullName, "Desktop"); }

			if (ShouldRestoreInternetExplorerFavorites) { CopyDirectoryContents(oldProfileDir.FullName, newProfileDir.FullName, "Favorites"); }

			if (ShouldRestoreOutlookSignatures) { CopyDirectoryContents(oldProfileDir.FullName, newProfileDir.FullName, @"AppData\Roaming\Microsoft\Signaturer"); }

			if (ShouldRestoreWindowsExplorerFavorites) { CopyDirectoryContents(oldProfileDir.FullName, newProfileDir.FullName, "Links"); }

			if (ShouldRestoreStickyNotes) { CopyDirectoryContents(oldProfileDir.FullName, newProfileDir.FullName, @"AppData\Roaming\Microsoft\Sticky Notes"); }
		});



		private DirectoryInfo GetProfileDirectory(string hostName) => hostName != null ? new DirectoryInfo($@"\\{hostName}\c$\Users") : null;

		private DirectoryInfo GetParentDirectory(string path) => path != null ? new DirectoryInfo(path).Parent : null;

		private void BangRenameDirectory(DirectoryInfo directory, string userName)
		{
			var destination = directory.FullName;

			while (Directory.Exists(destination))
			{
				destination = destination.Insert(destination.ToLowerInvariant().IndexOf(userName.ToLowerInvariant()), "!");
			}

			Directory.Move(directory.FullName, destination);
		}

		private IEnumerable<string> GetLoggedInUsers(string hostName)
		{
			var returnCollection = new List<string>();

			var conOptions = new ConnectionOptions();
			conOptions.Impersonation = ImpersonationLevel.Impersonate;
			conOptions.EnablePrivileges = true;

			var scope = new ManagementScope($"\\\\{hostName}\\ROOT\\CIMV2", conOptions);
			scope.Connect();

			var query = new ObjectQuery("SELECT * FROM Win32_Process where name='explorer.exe'");
			var searcher = new ManagementObjectSearcher(scope, query);

			foreach (ManagementObject item in searcher.Get())
			{
				var argsArray = new string[] { string.Empty };
				item.InvokeMethod("GetOwner", argsArray);
				returnCollection.Add(argsArray[0]);
			}

			return returnCollection;
		}

		private long PingNameOrAddressAsync(string nameOrAddress)
		{
			var pinger = new Ping();
			PingReply reply = null;

			try { reply = pinger.Send(nameOrAddress, 1000); }
			catch { /* Do nothing */ }

			if (reply.Status == IPStatus.Success) return reply.RoundtripTime;
			else return -1L;
		}

		private void CopyDirectoryContents(string selectedProfilePath, string newProfilePath, string subFolderPath)
		{
			var source = Path.Combine(selectedProfilePath, subFolderPath);
			var destination = Path.Combine(newProfilePath, subFolderPath);

			CopyFilesAndDirectories(source, destination);
		}

		private void CopyFilesAndDirectories(string source, string destination)
		{
			if (!Directory.Exists(source)) { return; }

			Directory.CreateDirectory(destination);

			foreach (var file in Directory.GetFiles(source).Select(x => new FileInfo(x)))
			{
				File.Copy(file.FullName, Path.Combine(destination, file.Name), true);
			}

			foreach (var directory in Directory.GetDirectories(source).Select(x => new DirectoryInfo(x)))
			{
				CopyFilesAndDirectories(directory.FullName, Path.Combine(destination, directory.Name));
			}
		}

		[DllImport("userenv.dll", CharSet = CharSet.Unicode, ExactSpelling = false, SetLastError = true)]
		public static extern bool DeleteProfile(string sidString, string profilePath, string computerName);

		private string CreateLogString(string logMessage) => $"{DateTimeOffset.Now.ToString("T")} - {logMessage}";
	}
}
