using Microsoft.Win32;
using ReactiveUI;
using SupportTool.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;

namespace SupportTool.ViewModels
{
    public class ProfilePanelViewModel : ViewModelBase
    {
        private readonly ReactiveCommand<Unit, Unit> _resetGlobalProfile;
        private readonly ReactiveCommand<Unit, Unit> _resetLocalProfile;
        private readonly ReactiveCommand<Unit, Tuple<DirectoryInfo, IEnumerable<DirectoryInfo>>> _searchForProfiles;
        private readonly ReactiveCommand<Unit, Unit> _restoreProfile;
        private readonly ReactiveCommand<Unit, Unit> _resetCitrixProfile;
        private readonly ReactiveCommand<Unit, Unit> _openGlobalProfile;
        private readonly ReactiveCommand<Unit, Unit> _openHomeFolder;
        private readonly ReactiveList<DirectoryInfo> _profiles = new ReactiveList<DirectoryInfo>();
        private readonly ObservableAsPropertyHelper<bool> _isExecutingResetGlobalProfile;
        private readonly ObservableAsPropertyHelper<bool> _isExecutingResetLocalProfile;
        private UserObject _user;
        private bool _isShowingResetProfile;
        private bool _isShowingRestoreProfile;
        private string _computerName;
        private bool _shouldRestoreDesktopItems = true;
        private bool _shouldRestoreInternetExplorerFavorites = true;
        private bool _shouldRestoreOutlookSignatures = true;
        private bool _shouldRestoreWindowsExplorerFavorites = true;
        private bool _shouldRestoreStickyNotes = true;
        private int _selectedProfileIndex;
        private DirectoryInfo _globalProfileDirectory;
        private DirectoryInfo _localProfileDirectory;
        private DirectoryInfo _newProfileDirectory;



        public ProfilePanelViewModel()
        {
            _resetGlobalProfile = ReactiveCommand.CreateFromObservable(() => ResetGlobalProfileImpl(_user));

            _resetLocalProfile = ReactiveCommand.CreateFromObservable(
                () => ResetLocalProfileImpl(_user, _computerName),
                this.WhenAnyValue(x => x.ComputerName, x => x.HasValue(6)));

            _searchForProfiles = ReactiveCommand.CreateFromObservable(
                () =>
                {
                    _profiles.Clear();
                    return SearchForProfilesImpl(_user, _computerName);
                },
                this.WhenAnyValue(x => x.ComputerName, x => x.HasValue()));

            _restoreProfile = ReactiveCommand.CreateFromObservable(
                () => RestoreProfileImpl(NewProfileDirectory, _profiles[SelectedProfileIndex]),
                this.WhenAnyValue(x => x.NewProfileDirectory, y => y.SelectedProfileIndex, (x, y) => x != null && y >= 0));

            _resetCitrixProfile = ReactiveCommand.CreateFromObservable(() => ResetCitrixProfileImpl(_user));

            _openGlobalProfile = ReactiveCommand.Create(
                () =>
                {
                    var profileDirectory = new DirectoryInfo(_user.ProfilePath);
                    Process.Start(profileDirectory.Parent.GetDirectories($"{profileDirectory.Name}*").LastOrDefault().FullName);
                });

            _openHomeFolder = ReactiveCommand.Create(() => { Process.Start(_user.HomeDirectory); });

            _isExecutingResetGlobalProfile = _resetGlobalProfile.IsExecuting
                .ToProperty(this, x => x.IsExecutingResetGlobalProfile);

            _isExecutingResetLocalProfile = _resetLocalProfile.IsExecuting
                .ToProperty(this, x => x.IsExecutingResetLocalProfile);

            this.WhenActivated(disposables =>
            {
                Observable.Merge(
                    _resetGlobalProfile.Select(_ => "Global profile reset"),
                    _resetLocalProfile.Select(_ => "Local profile reset"),
                    _restoreProfile.Select(_ => "Profile restored"),
                    _resetCitrixProfile.Select(_ => "Citrix profile reset"))
                    .SelectMany(x => _infoMessages.Handle(new MessageInfo(x, "Success")))
                    .Subscribe()
                    .DisposeWith(disposables);

                _searchForProfiles
                    .Subscribe(x =>
                    {
                        NewProfileDirectory = x.Item1;
                        using (_profiles.SuppressChangeNotifications()) _profiles.AddRange(x.Item2);
                    })
                    .DisposeWith(disposables);

                MessageBus.Current
                .Listen<string>(ApplicationActionRequest.SetLocalProfileComputerName.ToString())
                .Subscribe(x => ComputerName = x)
                .DisposeWith(disposables);

                this
                    .WhenAnyValue(x => x.IsShowingResetProfile)
                    .Where(x => x)
                    .Subscribe(_ => IsShowingRestoreProfile = false)
                    .DisposeWith(disposables);

                this
                    .WhenAnyValue(x => x.IsShowingRestoreProfile)
                    .Where(x => x)
                    .Subscribe(_ => IsShowingResetProfile = false)
                    .DisposeWith(disposables);

                Observable.Merge(
                    _resetGlobalProfile.ThrownExceptions,
                    _resetLocalProfile.ThrownExceptions,
                    _searchForProfiles.ThrownExceptions,
                    _restoreProfile.ThrownExceptions,
                    _resetCitrixProfile.ThrownExceptions,
                    _openGlobalProfile.ThrownExceptions,
                    _openHomeFolder.ThrownExceptions)
                    .SelectMany(ex => _errorMessages.Handle(new MessageInfo(ex.Message)))
                    .Subscribe()
                    .DisposeWith(disposables);
            });
        }



        public ReactiveCommand ResetGlobalProfile => _resetGlobalProfile;

        public ReactiveCommand ResetLocalProfile => _resetLocalProfile;

        public ReactiveCommand SearchForProfiles => _searchForProfiles;

        public ReactiveCommand RestoreProfile => _restoreProfile;

        public ReactiveCommand ResetCitrixProfile => _resetCitrixProfile;

        public ReactiveCommand OpenGlobalProfile => _openGlobalProfile;

        public ReactiveCommand OpenHomeFolder => _openHomeFolder;

        public ReactiveList<DirectoryInfo> Profiles => _profiles;

        public bool IsExecutingResetGlobalProfile => _isExecutingResetGlobalProfile.Value;

        public bool IsExecutingResetLocalProfile => _isExecutingResetLocalProfile.Value;

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



        private IObservable<Unit> ResetGlobalProfileImpl(UserObject usr) => Observable.Start(() =>
        {
            var globalProfileDirecotry = GetParentDirectory(usr.ProfilePath);
            if (globalProfileDirecotry == null) throw new Exception("Could not get profile path");

            foreach (var dir in globalProfileDirecotry.GetDirectories($"{usr.Principal.SamAccountName}*")) BangRenameDirectory(dir, usr.Principal.SamAccountName);
        });

        private IObservable<Unit> ResetLocalProfileImpl(UserObject usr, string cpr) => Observable.Start(() =>
        {
            if (PingNameOrAddressAsync(cpr) < 0) throw new Exception($"Could not connect to {cpr}");

            if (GetLoggedInUsers(cpr).Select(x => x.ToLowerInvariant()).Contains(usr.Principal.SamAccountName)) throw new Exception("User is logged in");

            var profileDir = GetProfileDirectory(cpr);
            foreach (var dir in profileDir.GetDirectories($"{usr.Principal.SamAccountName}*")) BangRenameDirectory(dir, usr.Principal.SamAccountName);

            var bracketedGuid = $"{{{usr.Principal.Guid.ToString()}}}";
            var userSid = usr.Principal.Sid.Value;

            var keyHive = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, $"{cpr}", RegistryView.Registry64);

            var profileListKey = keyHive.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\ProfileList", true);
            var groupPolicyKey = keyHive.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Group Policy", true);
            var groupPolicyStateKey = keyHive.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Group Policy\State", true);
            var userDataKey = keyHive.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Installer\UserData", true);
            var profileGuidKey = keyHive.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\ProfileGuid", true);
            var policyGuidKey = keyHive.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\PolicyGuid", true);

            if (userSid != null && profileListKey?.OpenSubKey(userSid) != null)
            {
                try { profileListKey.DeleteSubKeyTree(userSid); }
                catch { /* Do nothing */ }
            }

            if (userSid != null && groupPolicyKey?.OpenSubKey(userSid) != null)
            {
                try { groupPolicyKey.DeleteSubKeyTree(userSid); }
                catch { /* Do nothing */ }
            }

            if (userSid != null && groupPolicyStateKey?.OpenSubKey(userSid) != null)
            {
                try { groupPolicyStateKey.DeleteSubKeyTree(userSid); }
                catch { /* Do nothing */ }
            }

            if (userSid != null && userDataKey?.OpenSubKey(userSid) != null)
            {
                try { userDataKey.DeleteSubKeyTree(userSid); }
                catch { /* Do nothing */ }
            }

            if (bracketedGuid != null && profileGuidKey?.OpenSubKey(bracketedGuid) != null)
            {
                try { profileGuidKey.DeleteSubKeyTree(bracketedGuid); }
                catch { /* Do nothing */ }
            }

            if (bracketedGuid != null && policyGuidKey?.OpenSubKey(bracketedGuid) != null)
            {
                try { policyGuidKey.DeleteSubKeyTree(bracketedGuid); }
                catch { /* Do nothing */ }
            }
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

            if (ShouldRestoreStickyNotes) { CopyDirectoryContents(oldProfileDir.FullName, newProfileDir.FullName, @"AppData\Roaming\Microsoft\Sticky Notes"); }
        });

        private IObservable<Unit> ResetCitrixProfileImpl(UserObject user) => Observable.Start(() =>
        {
            foreach (var folderName in new[] { "xa_profile", "App-V" })
            {
                var destination = Path.Combine(user.HomeDirectory, "windows", folderName);
                if (!Directory.Exists(destination)) continue;

                while (Directory.Exists(destination))
                {
                    destination = destination.Insert(destination.ToLowerInvariant().IndexOf(folderName), "!");
                }

                Directory.Move(Path.Combine(user.HomeDirectory, "windows", folderName), destination);
            }
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

            var conOptions = new ConnectionOptions()
            {
                Impersonation = ImpersonationLevel.Impersonate,
                EnablePrivileges = true
            };
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
    }
}
