using Microsoft.Win32;
using ReactiveUI;
using SupportTool.Helpers;
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
using System.Threading;

namespace SupportTool.ViewModels
{
	public class ProfilePanelViewModel : ReactiveObject
	{
		private readonly ReactiveCommand<Unit, string> resetGlobalProfile;
		private readonly ReactiveCommand<Unit, string> resetLocalProfile;
		private readonly ReactiveCommand<Unit, Unit> openGlobalProfileDirectory;
		private readonly ReactiveCommand<Unit, Unit> openLocalProfileDirectory;
		private readonly ReactiveCommand<Unit, Tuple<DirectoryInfo, IEnumerable<DirectoryInfo>>> searchForProfiles;
		private readonly ReactiveCommand<Unit, Unit> restoreProfile;
		private readonly ReactiveList<string> resetMessages;
		private readonly ReactiveList<DirectoryInfo> profiles;
		private UserObject user;
		private bool isShowingResetProfile;
		private bool isShowingRestoreProfile;
		private string computerName;
		private bool shouldRestoreDesktopItems;
		private bool shouldRestoreInternetExplorerFavorites;
		private bool shouldRestoreOutlookSignatures;
		private bool shouldRestoreWindowsExplorerFavorites;
		private bool shouldRestoreStickyNotes;
		private int selectedProfileIndex;
		private DirectoryInfo globalProfileDirectory;
		private DirectoryInfo localProfileDirectory;
		private DirectoryInfo newProfileDirectory;



		public ProfilePanelViewModel()
		{
			resetMessages = new ReactiveList<string>();
			profiles = new ReactiveList<DirectoryInfo>();

			resetGlobalProfile = ReactiveCommand.CreateFromObservable(() => ResetGlobalProfileImpl(user));
			resetGlobalProfile
				.Subscribe(x => resetMessages.Insert(0, x));
			resetGlobalProfile
				.ThrownExceptions
				.Subscribe(ex =>
				{
					resetMessages.Insert(0, CreateLogString("Could not reset global profile"));
					DialogService.ShowError(ex.Message);
				});

			resetLocalProfile = ReactiveCommand.CreateFromObservable(
				() => ResetLocalProfileImpl(user, computerName),
				this.WhenAnyValue(x => x.ComputerName, x => x.HasValue(6)));
			resetLocalProfile
				.Subscribe(x => resetMessages.Insert(0, x));
			resetLocalProfile
				.ThrownExceptions
				.Subscribe(ex =>
				{
					resetMessages.Insert(0, CreateLogString("Could not reset local profile"));
					DialogService.ShowError(ex.Message);
				});

			openGlobalProfileDirectory = ReactiveCommand.Create(
				() =>
				{
					var gpdPath = GetParentDirectory(user.ProfilePath);
					if (gpdPath == null || !gpdPath.Exists) throw new Exception($"Could not find global profile folder");
					Process.Start(gpdPath.FullName);
				});
			openGlobalProfileDirectory
				.ThrownExceptions
				.Subscribe(ex => DialogService.ShowError(ex.Message, "Could not open directory"));

			openLocalProfileDirectory = ReactiveCommand.Create(
				() =>
				{
					var lpd = GetProfileDirectory(ComputerName);
					if (lpd == null || !lpd.Exists) throw new Exception($"Could not find local profile folder");
					Process.Start(lpd.FullName);
				},
				this.WhenAnyValue(x => x.ComputerName, x => x.HasValue(6)));
			openLocalProfileDirectory
				.ThrownExceptions
				.Subscribe(ex => DialogService.ShowError(ex.Message, "Could not open directory"));

			searchForProfiles = ReactiveCommand.CreateFromObservable(() =>
			{
				profiles.Clear();
				return SearchForProfilesImpl(user, computerName);
			},
			this.WhenAnyValue(x => x.ComputerName, x => x.HasValue()));
			searchForProfiles
				.Subscribe(x =>
				{
					NewProfileDirectory = x.Item1;
					using (profiles.SuppressChangeNotifications()) profiles.AddRange(x.Item2);
				});
			searchForProfiles
				.ThrownExceptions
				.Subscribe(ex => DialogService.ShowError(ex.Message));

			restoreProfile = ReactiveCommand.CreateFromObservable(
				() => RestoreProfileImpl(NewProfileDirectory, profiles[SelectedProfileIndex]),
				this.WhenAnyValue(x => x.NewProfileDirectory, y => y.SelectedProfileIndex, (x, y) => x != null && y >= 0));
			restoreProfile
				.Subscribe(_ => DialogService.ShowInfo("Profile restored", "Success"));
			restoreProfile
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

			this
				.WhenAnyValue(x => x.User)
				.Subscribe(_ => ResetValues());
		}



		public ReactiveCommand ResetGlobalProfile => resetGlobalProfile;

		public ReactiveCommand ResetLocalProfile => resetLocalProfile;

		public ReactiveCommand OpenGlobalProfileDirectory => openGlobalProfileDirectory;

		public ReactiveCommand OpenLocalProfileDirectory => openLocalProfileDirectory;

		public ReactiveCommand SearchForProfiles => searchForProfiles;

		public ReactiveCommand RestoreProfile => restoreProfile;

		public ReactiveList<string> ResetMessages => resetMessages;

		public ReactiveList<DirectoryInfo> Profiles => profiles;

		public UserObject User
		{
			get { return user; }
			set { this.RaiseAndSetIfChanged(ref user, value); }
		}

		public bool IsShowingResetProfile
		{
			get { return isShowingResetProfile; }
			set { this.RaiseAndSetIfChanged(ref isShowingResetProfile, value); }
		}

		public bool IsShowingRestoreProfile
		{
			get { return isShowingRestoreProfile; }
			set { this.RaiseAndSetIfChanged(ref isShowingRestoreProfile, value); }
		}

		public string ComputerName
		{
			get { return computerName; }
			set { this.RaiseAndSetIfChanged(ref computerName, value); }
		}

		public bool ShouldRestoreDesktopItems
		{
			get { return shouldRestoreDesktopItems; }
			set { this.RaiseAndSetIfChanged(ref shouldRestoreDesktopItems, value); }
		}

		public bool ShouldRestoreInternetExplorerFavorites
		{
			get { return shouldRestoreInternetExplorerFavorites; }
			set { this.RaiseAndSetIfChanged(ref shouldRestoreInternetExplorerFavorites, value); }
		}

		public bool ShouldRestoreOutlookSignatures
		{
			get { return shouldRestoreOutlookSignatures; }
			set { this.RaiseAndSetIfChanged(ref shouldRestoreOutlookSignatures, value); }
		}

		public bool ShouldRestoreWindowsExplorerFavorites
		{
			get { return shouldRestoreWindowsExplorerFavorites; }
			set { this.RaiseAndSetIfChanged(ref shouldRestoreWindowsExplorerFavorites, value); }
		}

		public bool ShouldRestoreStickyNotes
		{
			get { return shouldRestoreStickyNotes; }
			set { this.RaiseAndSetIfChanged(ref shouldRestoreStickyNotes, value); }
		}

		public int SelectedProfileIndex
		{
			get { return selectedProfileIndex; }
			set { this.RaiseAndSetIfChanged(ref selectedProfileIndex, value); }
		}

		public DirectoryInfo GlobalProfileDirectory
		{
			get { return globalProfileDirectory; }
			set { this.RaiseAndSetIfChanged(ref globalProfileDirectory, value); }
		}

		public DirectoryInfo LocalProfileDirectory
		{
			get { return localProfileDirectory; }
			set { this.RaiseAndSetIfChanged(ref localProfileDirectory, value); }
		}

		public DirectoryInfo NewProfileDirectory
		{
			get { return newProfileDirectory; }
			set { this.RaiseAndSetIfChanged(ref newProfileDirectory, value); }
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

			var bracketedGuid = $"{{{user.Principal.Guid.ToString()}}}";
			var userSid = user.Principal.Sid.Value;

			var keyHive = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, $"{cpr}");

			var profileListKey = keyHive.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\ProfileList", true);
			var groupPolicyKey = keyHive.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Group Policy", true);
			var groupPolicyStateKey = keyHive.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Group Policy\State", true);
			var userDataKey = keyHive.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Installer\UserData", true);
			var profileGuidKey = keyHive.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\ProfileGuid", true);
			var policyGuidKey = keyHive.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\PolicyGuid", true);

			if (profileListKey?.OpenSubKey(userSid) != null)
			{
				try
				{
					profileListKey.DeleteSubKeyTree(userSid);
					observer.OnNext(CreateLogString($"Deleted key in {profileListKey.Name}"));
				}
				catch { observer.OnNext(CreateLogString($"Couldn't delete key in {profileListKey.Name}")); }
			}
			else { observer.OnNext(CreateLogString($"Didn't find key in {profileListKey.Name}")); }

			if (groupPolicyKey?.OpenSubKey(userSid) != null)
			{
				try
				{
					groupPolicyKey.DeleteSubKeyTree(userSid);
					observer.OnNext(CreateLogString($"Deleted key in {groupPolicyKey.Name}"));
				}
				catch { observer.OnNext(CreateLogString($"Couldn't delete key in {groupPolicyKey.Name}")); }
			}
			else { observer.OnNext(CreateLogString($"Didn't find key in {groupPolicyKey.Name}")); }

			if (groupPolicyStateKey?.OpenSubKey(userSid) != null)
			{
				try
				{
					groupPolicyStateKey.DeleteSubKeyTree(userSid);
					observer.OnNext(CreateLogString($"Deleted key in {groupPolicyStateKey.Name}"));
				}
				catch { observer.OnNext(CreateLogString($"Couldn't delete key in {groupPolicyStateKey.Name}")); }
			}
			else { observer.OnNext(CreateLogString($"Didn't find key in {groupPolicyStateKey.Name}")); }

			if (userDataKey?.OpenSubKey(userSid) != null)
			{
				try
				{
					userDataKey.DeleteSubKeyTree(userSid);
					observer.OnNext(CreateLogString($"Deleted key in {userDataKey.Name}"));
				}
				catch { observer.OnNext(CreateLogString($"Couldn't delete key in {userDataKey.Name}")); }
			}
			else { observer.OnNext(CreateLogString($"Didn't find key in {userDataKey.Name}")); }

			if (profileGuidKey?.OpenSubKey(bracketedGuid) != null)
			{
				try
				{
					profileGuidKey.DeleteSubKeyTree(bracketedGuid);
					observer.OnNext(CreateLogString($"Deleted key in {profileGuidKey.Name}"));
				}
				catch { observer.OnNext(CreateLogString($"Couldn't delete key in {profileGuidKey.Name}")); }
			}
			else { observer.OnNext(CreateLogString($"Didn't find key in {profileGuidKey.Name}")); }

			if (policyGuidKey?.OpenSubKey(bracketedGuid) != null)
			{
				try
				{
					policyGuidKey.DeleteSubKeyTree(bracketedGuid);
					observer.OnNext(CreateLogString($"Deleted key in {policyGuidKey.Name}"));
				}
				catch { observer.OnNext(CreateLogString($"Couldn't delete key in {policyGuidKey.Name}")); }
			}
			else { observer.OnNext(CreateLogString($"Didn't find key in {policyGuidKey.Name}")); }

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

		private string CreateLogString(string logMessage) => $"{DateTimeOffset.Now.ToString("T")} - {logMessage}";



		private void ResetValues()
		{
			ResetMessages.Clear();
			Profiles.Clear();
			IsShowingResetProfile = false;
			IsShowingRestoreProfile = false;
			ComputerName = "";
			ShouldRestoreDesktopItems = true;
			ShouldRestoreInternetExplorerFavorites = true;
			ShouldRestoreOutlookSignatures = true;
			ShouldRestoreWindowsExplorerFavorites = true;
			ShouldRestoreStickyNotes = true;
			SelectedProfileIndex = -1;
			LocalProfileDirectory = null;
			GlobalProfileDirectory = null;
		}
	}
}
