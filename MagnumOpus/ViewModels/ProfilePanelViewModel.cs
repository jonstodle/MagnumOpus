using Microsoft.Win32;
using ReactiveUI;
using MagnumOpus.Models;
using MagnumOpus.Services.ActiveDirectoryServices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Reactive.Concurrency;

namespace MagnumOpus.ViewModels
{
    public class ProfilePanelViewModel : ViewModelBase
    {
        public ProfilePanelViewModel()
        {
            ResetGlobalProfile = ReactiveCommand.CreateFromObservable(() => ResetGlobalProfileImpl(_user));

            ResetLocalProfile = ReactiveCommand.CreateFromObservable(
                () => ResetLocalProfileImpl(_user, _computerName),
                this.WhenAnyValue(x => x.ComputerName, x => x.HasValue(6)));

            SearchForProfiles = ReactiveCommand.CreateFromObservable(
                () =>
                {
                    _profiles.Clear();
                    return SearchForProfilesImpl(_user, _computerName);
                },
                this.WhenAnyValue(x => x.ComputerName, x => x.HasValue()));

            RestoreProfile = ReactiveCommand.CreateFromObservable(
                () => RestoreProfileImpl(NewProfileDirectory, _profiles[SelectedProfileIndex]),
                this.WhenAnyValue(x => x.NewProfileDirectory, y => y.SelectedProfileIndex, (x, y) => x != null && y >= 0));

            ResetCitrixProfile = ReactiveCommand.CreateFromObservable(() => ResetCitrixProfileImpl(_user));

            OpenGlobalProfile = ReactiveCommand.Create(
                () =>
                {
                    var profileDirectory = new DirectoryInfo(_user.ProfilePath);
                    Process.Start(profileDirectory.Parent.GetDirectories($"{profileDirectory.Name}*").LastOrDefault().FullName);
                },
                this.WhenAnyValue(x => x.GlobalProfilePath, x => x.HasValue()));

            SaveGlobalProfilePath = ReactiveCommand.CreateFromObservable(
                () => Observable.Start(() =>
                {
                    var de = _user.Principal.GetUnderlyingObject() as DirectoryEntry;
                    de.Properties["profilepath"].Value = _globalProfilePath.HasValue() ? _globalProfilePath : null;
                    de.CommitChanges();
                    MessageBus.Current.SendMessage(_user.CN, ApplicationActionRequest.Refresh);
                }, TaskPoolScheduler.Default));

            CancelGlobalProfilePath = ReactiveCommand.Create(() => { GlobalProfilePath = _user.ProfilePath; });

            OpenHomeFolder = ReactiveCommand.Create(
                () => { Process.Start(_user.HomeDirectory); },
                this.WhenAnyValue(x => x.HomeFolderPath, x => x.HasValue()));

            SaveHomeFolderPath = ReactiveCommand.CreateFromObservable(
                () => Observable.Start(() =>
                {
                    var p = _user.Principal;
                    p.HomeDirectory = _homeFolderPath.HasValue() ? _homeFolderPath : null;
                    p.Save();
                    MessageBus.Current.SendMessage(_user.CN, ApplicationActionRequest.Refresh);
                }, TaskPoolScheduler.Default));

            CancelHomeFolderPath = ReactiveCommand.Create(() => { HomeFolderPath = _user.HomeDirectory; });

            _isExecutingResetGlobalProfile = ResetGlobalProfile.IsExecuting
                .ToProperty(this, x => x.IsExecutingResetGlobalProfile);

            _isExecutingResetLocalProfile = ResetLocalProfile.IsExecuting
                .ToProperty(this, x => x.IsExecutingResetLocalProfile);

            _isExecutingRestoreProfile = RestoreProfile.IsExecuting
                .ToProperty(this, x => x.IsExecutingRestoreProfile);

            _hasGlobalProfilePathChanged = Observable.CombineLatest(
                this.WhenAnyValue(x => x.GlobalProfilePath),
                this.WhenAnyValue(y => y.User).WhereNotNull(),
                (x, y) => x != y.ProfilePath)
                .ToProperty(this, x => x.HasGlobalProfilePathChanged);

            _hasHomeFolderPathChanged = Observable.CombineLatest(
                this.WhenAnyValue(x => x.HomeFolderPath),
                this.WhenAnyValue(y => y.User).WhereNotNull(),
                (x, y) => x != y.HomeDirectory)
                .ToProperty(this, x => x.HasHomeFolderPathChanged);

            (this).WhenActivated((Action<CompositeDisposable>)(disposables =>
            {
                var userChanged = (this).WhenAnyValue(x => x.User)
                    .WhereNotNull()
                    .Subscribe(x =>
                    {
                        GlobalProfilePath = x.ProfilePath;
                        HomeFolderPath = x.HomeDirectory;
                    })
                    .DisposeWith(disposables);

                Observable.Merge(
                    Observable.Select<Unit, string>(this.ResetGlobalProfile, (Func<Unit, string>)(_ => (string)"Global profile reset")),
                    ResetLocalProfile.Select(_ => "Local profile reset"),
                    RestoreProfile.Select(_ => "Profile restored"),
                    ResetCitrixProfile.Select(_ => "Citrix profile reset"))
                    .SelectMany(x => _messages.Handle(new MessageInfo(MessageType.Success, x, "Success")))
                    .Subscribe()
                    .DisposeWith(disposables);

                SearchForProfiles
                    .Subscribe((Tuple<DirectoryInfo, IEnumerable<DirectoryInfo>> x) =>
                    {
                        NewProfileDirectory = x.Item1;
                        using (_profiles.SuppressChangeNotifications()) _profiles.AddRange(x.Item2);
                    })
                    .DisposeWith(disposables);

                MessageBus.Current
                .Listen<string>(ApplicationActionRequest.SetLocalProfileComputerName)
                .Subscribe(x => ComputerName = x)
                .DisposeWith(disposables);

                Observable.Merge(
                    (this).WhenAnyValue(x => x.IsShowingResetProfile).Where(x => x).Select(_ => Tuple.Create(true, false, false, false)),
                    (this).WhenAnyValue(x => x.IsShowingRestoreProfile).Where(x => x).Select(_ => Tuple.Create(false, true, false, false)),
                    (this).WhenAnyValue(x => x.IsShowingGlobalProfile).Where(x => x).Select(_ => Tuple.Create(false, false, true, false)),
                    (this).WhenAnyValue(x => x.IsShowingHomeFolder).Where(x => x).Select(_ => Tuple.Create(false, false, false, true)))
                    .Subscribe((Tuple<bool, bool, bool, bool> x) =>
                    {
                        IsShowingResetProfile = x.Item1;
                        IsShowingRestoreProfile = x.Item2;
                        IsShowingGlobalProfile = x.Item3;
                        IsShowingHomeFolder = x.Item4;
                    })
                    .DisposeWith(disposables);

                Observable.Merge(
                    Observable.Select<Exception, (string, string)>(this.ResetGlobalProfile.ThrownExceptions, (Func<Exception, (string, string)>)(ex => ((string, string))(((string)"Could not reset global profile", (string)ex.Message)))),
                    ResetLocalProfile.ThrownExceptions.Select(ex => (("Could not reset local profile", ex.Message))),
                    SearchForProfiles.ThrownExceptions.Select(ex => (("Could not complete search", ex.Message))),
                    RestoreProfile.ThrownExceptions.Select(ex => (("Could not restore profile", ex.Message))),
                    ResetCitrixProfile.ThrownExceptions.Select(ex => (("Could not reset Citrix profile", ex.Message))),
                    OpenGlobalProfile.ThrownExceptions.Select(ex => (("Could not open global profile", ex.Message))),
                    SaveGlobalProfilePath.ThrownExceptions.Select(ex => (("Could not save changes", ex.Message))),
                    CancelGlobalProfilePath.ThrownExceptions.Select(ex => (("Could not reverse changes", ex.Message))),
                    OpenHomeFolder.ThrownExceptions.Select(ex => (("Could not home folder", ex.Message))),
                    SaveHomeFolderPath.ThrownExceptions.Select(ex => (("Could not save changes", ex.Message))),
                    CancelHomeFolderPath.ThrownExceptions.Select(ex => (("Could not reverse changes", ex.Message))))
                    .SelectMany(((string, string) x) => _messages.Handle(new MessageInfo(MessageType.Error, x.Item2, x.Item1)))
                    .Subscribe()
                    .DisposeWith(disposables);
            }));
        }



        public ReactiveCommand<Unit, Unit> ResetGlobalProfile { get; private set; }
        public ReactiveCommand<Unit, Unit> ResetLocalProfile { get; private set; }
        public ReactiveCommand<Unit, Tuple<DirectoryInfo, IEnumerable<DirectoryInfo>>> SearchForProfiles { get; private set; }
        public ReactiveCommand<Unit, Unit> RestoreProfile { get; private set; }
        public ReactiveCommand<Unit, Unit> ResetCitrixProfile { get; private set; }
        public ReactiveCommand<Unit, Unit> OpenGlobalProfile { get; private set; }
        public ReactiveCommand<Unit, Unit> SaveGlobalProfilePath { get; private set; }
        public ReactiveCommand<Unit, Unit> CancelGlobalProfilePath { get; private set; }
        public ReactiveCommand<Unit, Unit> OpenHomeFolder { get; private set; }
        public ReactiveCommand<Unit, Unit> SaveHomeFolderPath { get; private set; }
        public ReactiveCommand<Unit, Unit> CancelHomeFolderPath { get; private set; }
        public ReactiveList<DirectoryInfo> Profiles => _profiles;
        public bool IsExecutingResetGlobalProfile => _isExecutingResetGlobalProfile.Value;
        public bool IsExecutingResetLocalProfile => _isExecutingResetLocalProfile.Value;
        public bool IsExecutingRestoreProfile => _isExecutingRestoreProfile.Value;
        public bool HasGlobalProfilePathChanged => _hasGlobalProfilePathChanged.Value;
        public bool HasHomeFolderPathChanged => _hasHomeFolderPathChanged.Value;
        public UserObject User { get => _user; set => this.RaiseAndSetIfChanged(ref _user, value); }
        public bool IsShowingResetProfile { get => _isShowingResetProfile; set => this.RaiseAndSetIfChanged(ref _isShowingResetProfile, value); }
        public bool IsShowingRestoreProfile { get => _isShowingRestoreProfile; set => this.RaiseAndSetIfChanged(ref _isShowingRestoreProfile, value); }
        public bool IsShowingGlobalProfile { get => _isShowingGlobalProfile; set => this.RaiseAndSetIfChanged(ref _isShowingGlobalProfile, value); }
        public bool IsShowingHomeFolder { get => _isShowingHomeFolder; set => this.RaiseAndSetIfChanged(ref _isShowingHomeFolder, value); }
        public string ComputerName { get => _computerName; set => this.RaiseAndSetIfChanged(ref _computerName, value); }
        public bool ShouldRestoreDesktopItems { get => _shouldRestoreDesktopItems; set => this.RaiseAndSetIfChanged(ref _shouldRestoreDesktopItems, value); }
        public bool ShouldRestoreInternetExplorerFavorites { get => _shouldRestoreInternetExplorerFavorites; set => this.RaiseAndSetIfChanged(ref _shouldRestoreInternetExplorerFavorites, value); }
        public bool ShouldRestoreOutlookSignatures { get => _shouldRestoreOutlookSignatures; set => this.RaiseAndSetIfChanged(ref _shouldRestoreOutlookSignatures, value); }
        public bool ShouldRestoreWindowsExplorerFavorites { get => _shouldRestoreWindowsExplorerFavorites; set => this.RaiseAndSetIfChanged(ref _shouldRestoreWindowsExplorerFavorites, value); }
        public bool ShouldRestoreStickyNotes { get => _shouldRestoreStickyNotes; set => this.RaiseAndSetIfChanged(ref _shouldRestoreStickyNotes, value); }
        public int SelectedProfileIndex { get => _selectedProfileIndex; set => this.RaiseAndSetIfChanged(ref _selectedProfileIndex, value); }
        public string GlobalProfilePath { get => _globalProfilePath; set => this.RaiseAndSetIfChanged(ref _globalProfilePath, value); }
        public string HomeFolderPath { get => _homeFolderPath; set => this.RaiseAndSetIfChanged(ref _homeFolderPath, value); }
        public DirectoryInfo GlobalProfileDirectory { get => _globalProfileDirectory; set => this.RaiseAndSetIfChanged(ref _globalProfileDirectory, value); }
        public DirectoryInfo LocalProfileDirectory { get => _localProfileDirectory; set => this.RaiseAndSetIfChanged(ref _localProfileDirectory, value); }
        public DirectoryInfo NewProfileDirectory { get => _newProfileDirectory; set => this.RaiseAndSetIfChanged(ref _newProfileDirectory, value); }



        private IObservable<Unit> ResetGlobalProfileImpl(UserObject usr) => Observable.Start(() =>
        {
            var globalProfileDirecotry = GetParentDirectory(usr.ProfilePath);
            if (globalProfileDirecotry == null) throw new Exception("Could not get profile path");

            foreach (var dir in globalProfileDirecotry.GetDirectories($"{usr.Principal.SamAccountName}*")) BangRenameDirectory(dir, usr.Principal.SamAccountName);
        }, TaskPoolScheduler.Default);

        private IObservable<Unit> ResetLocalProfileImpl(UserObject usr, string cpr) => Observable.Start(() =>
        {
            if (PingNameOrAddressAsync(cpr) < 0) throw new Exception($"Could not connect to {cpr}");

            if (ActiveDirectoryService.Current.GetComputer(cpr).SelectMany(x => x.GetLoggedInUsers()).ToEnumerable().Select(x => x.Username.ToLowerInvariant()).Contains(usr.Principal.SamAccountName.ToLowerInvariant())) throw new Exception("User is logged in");

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
        }, TaskPoolScheduler.Default);

        private IObservable<Tuple<DirectoryInfo, IEnumerable<DirectoryInfo>>> SearchForProfilesImpl(UserObject usr, string cpr) => Observable.Start(() =>
        {
            if (PingNameOrAddressAsync(cpr) < 0) throw new Exception($"Could not connect to {cpr}");

            var profilesDirectory = new DirectoryInfo($@"\\{cpr}\C$\Users");
            var profileDirectories = profilesDirectory.GetDirectories($"*{usr.Principal.SamAccountName}*").ToList();

            var profileDir = profileDirectories.FirstOrDefault(x => x.Name.ToLowerInvariant() == usr.Principal.SamAccountName.ToLowerInvariant());
            if (profileDir == null) throw new Exception("No local profile folder");
            profileDirectories.Remove(profileDir);

            return Tuple.Create(profileDir, profileDirectories.AsEnumerable());
        }, TaskPoolScheduler.Default);

        private IObservable<Unit> RestoreProfileImpl(DirectoryInfo newProfileDir, DirectoryInfo oldProfileDir) => Observable.Start(() =>
        {
            if (ShouldRestoreDesktopItems) { CopyDirectoryContents(oldProfileDir.FullName, newProfileDir.FullName, "Desktop"); }

            if (ShouldRestoreInternetExplorerFavorites) { CopyDirectoryContents(oldProfileDir.FullName, newProfileDir.FullName, "Favorites"); }

            if (ShouldRestoreOutlookSignatures) { CopyDirectoryContents(oldProfileDir.FullName, newProfileDir.FullName, @"AppData\Roaming\Microsoft\Signaturer"); }

            if (ShouldRestoreWindowsExplorerFavorites) { CopyDirectoryContents(oldProfileDir.FullName, newProfileDir.FullName, "Links"); }

            if (ShouldRestoreStickyNotes) { CopyDirectoryContents(oldProfileDir.FullName, newProfileDir.FullName, @"AppData\Roaming\Microsoft\Sticky Notes"); }
        }, TaskPoolScheduler.Default);

        private IObservable<Unit> ResetCitrixProfileImpl(UserObject user) => Messages.Handle(new MessageInfo(MessageType.Question, "Do you want to reset the Citrix profile?", "", "Yes", "No"))
            .Where(result => result == 0)
            .SelectMany(_ => Observable.Start(() =>
                {
                    if (!Directory.Exists(Path.Combine(user.HomeDirectory, "windows"))) throw new ArgumentException("Could not find Citrix profile directory");

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
                }, TaskPoolScheduler.Default));



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

        private long PingNameOrAddressAsync(string nameOrAddress)
        {
            var pinger = new Ping();
            PingReply reply = null;

            try { reply = pinger.Send(nameOrAddress, 1000); }
            catch { /* Do nothing */ }

            if (reply?.Status == IPStatus.Success) return reply.RoundtripTime;
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



        private readonly ReactiveList<DirectoryInfo> _profiles = new ReactiveList<DirectoryInfo>();
        private readonly ObservableAsPropertyHelper<bool> _isExecutingResetGlobalProfile;
        private readonly ObservableAsPropertyHelper<bool> _isExecutingResetLocalProfile;
        private readonly ObservableAsPropertyHelper<bool> _isExecutingRestoreProfile;
        private readonly ObservableAsPropertyHelper<bool> _hasGlobalProfilePathChanged;
        private readonly ObservableAsPropertyHelper<bool> _hasHomeFolderPathChanged;
        private UserObject _user;
        private bool _isShowingResetProfile;
        private bool _isShowingRestoreProfile;
        private bool _isShowingGlobalProfile;
        private bool _isShowingHomeFolder;
        private string _computerName;
        private bool _shouldRestoreDesktopItems = true;
        private bool _shouldRestoreInternetExplorerFavorites = true;
        private bool _shouldRestoreOutlookSignatures = true;
        private bool _shouldRestoreWindowsExplorerFavorites = true;
        private bool _shouldRestoreStickyNotes = true;
        private int _selectedProfileIndex;
        private string _globalProfilePath;
        private string _homeFolderPath;
        private DirectoryInfo _globalProfileDirectory;
        private DirectoryInfo _localProfileDirectory;
        private DirectoryInfo _newProfileDirectory;
    }
}
