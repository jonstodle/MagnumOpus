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
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace SupportTool.ViewModels
{
    public class ProfilePanelViewModel : ReactiveObject
    {
        private readonly ReactiveCommand<Unit, string> resetGlobalProfile;
        private readonly ReactiveCommand<Unit, string> resetLocalProfile;
        private readonly ReactiveCommand<Unit, Process> openGlobalProfileDirectory;
        private readonly ReactiveCommand<Unit, Process> openLocalProfileDirectory;
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
        private int selectedProfileIndex;
        private DirectoryInfo globalProfileDirectory;
        private DirectoryInfo localProfileDirectory;



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
                () => Process.Start(GlobalProfileDirectory.FullName),
                this.WhenAnyValue(x => x.GlobalProfileDirectory).Select(x => x != null));

            openLocalProfileDirectory = ReactiveCommand.Create(
                () => Process.Start(LocalProfileDirectory.FullName),
                this.WhenAnyValue(x => x.LocalProfileDirectory).Select(x => x != null));

            searchForProfiles = ReactiveCommand.CreateFromObservable(() =>
            {
                profiles.Clear();
                return SearchForProfilesImpl(user, computerName);
            },
            this.WhenAnyValue(x => x.ComputerName, x => x.HasValue()));
            searchForProfiles
                .Subscribe(x =>
                {
                    localProfileDirectory = x.Item1;
                    using (profiles.SuppressChangeNotifications()) profiles.AddRange(x.Item2);
                });
            searchForProfiles
                .ThrownExceptions
                .Subscribe(ex => DialogService.ShowError(ex.Message));

            restoreProfile = ReactiveCommand.CreateFromObservable(() => RestoreProfileImpl(localProfileDirectory, profiles[SelectedProfileIndex]));
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



        private IObservable<string> ResetGlobalProfileImpl(UserObject usr) => Observable.Create<string>(observer =>
        {
            observer.OnNext(CreateLogString("Resetting global profile"));

            var profilePath = usr.ProfilePath;
            if (profilePath == null) throw new Exception("Could not get profile path");

            GlobalProfileDirectory = new DirectoryInfo(profilePath).Parent;
            foreach (var dir in GlobalProfileDirectory.GetDirectories($"{usr.Principal.SamAccountName}*"))
            {
                BangRenameDirectory(dir, usr.Principal.SamAccountName);
                observer.OnNext(CreateLogString($"Renamed folder {dir.FullName}"));
            }

            observer.OnNext(CreateLogString("Successfully reset global profile"));
            observer.OnCompleted();
            return () => { };
        });

        private IObservable<string> ResetLocalProfileImpl(UserObject usr, string cpr) => Observable.Create<string>(observer =>
        {
            if (PingNameOrAddressAsync(cpr) < 0) throw new Exception($"Could not connect to {cpr}");
            observer.OnNext(CreateLogString("Computer found"));

            if (GetLoggedInUsers(cpr).Select(x => x.ToLowerInvariant()).Contains(usr.Principal.SamAccountName)) throw new Exception("User is logged in");
            observer.OnNext(CreateLogString("Confirmed user is not logged in"));

            observer.OnNext(CreateLogString("Resetting local profile"));

            LocalProfileDirectory = new DirectoryInfo($@"\\{cpr}\c$\Users");
            foreach (var dir in LocalProfileDirectory.GetDirectories($"{usr.Principal.SamAccountName}*"))
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
                profileListKey.DeleteSubKeyTree(userSid);
                observer.OnNext(CreateLogString($"Deleted key in {profileListKey.Name}"));
            }
            else { observer.OnNext(CreateLogString($"Didn't find key in {profileListKey.Name}")); }

            if (groupPolicyKey?.OpenSubKey(userSid) != null)
            {
                groupPolicyKey.DeleteSubKeyTree(userSid);
                observer.OnNext(CreateLogString($"Deleted key in {groupPolicyKey.Name}"));
            }
            else { observer.OnNext(CreateLogString($"Didn't find key in {groupPolicyKey.Name}")); }

            if (groupPolicyStateKey?.OpenSubKey(userSid) != null)
            {
                groupPolicyStateKey.DeleteSubKeyTree(userSid);
                observer.OnNext(CreateLogString($"Deleted key in {groupPolicyStateKey.Name}"));
            }
            else { observer.OnNext(CreateLogString($"Didn't find key in {groupPolicyStateKey.Name}")); }

            if (userDataKey?.OpenSubKey(userSid) != null)
            {
                userDataKey.DeleteSubKeyTree(userSid);
                observer.OnNext(CreateLogString($"Deleted key in {userDataKey.Name}"));
            }
            else { observer.OnNext(CreateLogString($"Didn't find key in {userDataKey.Name}")); }

            if (profileGuidKey?.OpenSubKey(bracketedGuid) != null)
            {
                profileGuidKey.DeleteSubKeyTree(bracketedGuid);
                observer.OnNext(CreateLogString($"Deleted key in {profileGuidKey.Name}"));
            }
            else { observer.OnNext(CreateLogString($"Didn't find key in {profileGuidKey.Name}")); }

            if (policyGuidKey?.OpenSubKey(bracketedGuid) != null)
            {
                policyGuidKey.DeleteSubKeyTree(bracketedGuid);
                observer.OnNext(CreateLogString($"Deleted key in {policyGuidKey.Name}"));
            }
            else { observer.OnNext(CreateLogString($"Didn't find key in {policyGuidKey.Name}")); }

            observer.OnNext(CreateLogString("Successfully reset local profile"));
            observer.OnCompleted();
            return () => { };
        });

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
        });



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
            SelectedProfileIndex = -1;
            LocalProfileDirectory = null;
            GlobalProfileDirectory = null;
        }
    }
}
