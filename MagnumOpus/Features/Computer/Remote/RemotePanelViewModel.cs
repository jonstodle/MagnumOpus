using Microsoft.Win32;
using ReactiveUI;
using System;
using System.IO;
using System.Net.NetworkInformation;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Management.Automation;
using MagnumOpus.Dialog;
using MagnumOpus.Navigation;
using MagnumOpus.User;

using static MagnumOpus.FileHelpers.ExecutionService;

namespace MagnumOpus.Computer
{
    public class RemotePanelViewModel : ViewModelBase
    {
        public RemotePanelViewModel()
        {
            OpenUser = ReactiveCommand.CreateFromObservable(() => Observable.FromAsync(() => NavigationService.ShowWindow<UserWindow>(Tuple.Create(_selectedLoggedOnUser.Username, _hostName))));

            CopyUserName = ReactiveCommand.Create(() => Clipboard.SetText(_selectedLoggedOnUser.Username));

            LogOffUser = ReactiveCommand.CreateFromObservable(() => LogOffUserImpl(_selectedLoggedOnUser.SessionID, HostName));

            StartRemoteControl = ReactiveCommand.CreateFromObservable(() => StartRemoteControlImpl(_hostName));

            StartRemoteControlClassic = ReactiveCommand.CreateFromObservable(() => StartRemoteControlClassicImpl(_hostName));

            StartRemoteControl2012 = ReactiveCommand.CreateFromObservable(() => StartRemoteControl2012Impl(_hostName));

            StartRemoteAssistance = ReactiveCommand.CreateFromObservable(() => StartRemoteAssistanceImpl(_hostName));

            KillRemoteTools = ReactiveCommand.CreateFromObservable(() => KillRemoteToolsImpl(_hostName));

            ToggleUac = ReactiveCommand.CreateFromObservable(() => ToggleUacImpl(_hostName));

            StartRdp = ReactiveCommand.Create(() => RunFile(Path.Combine(System32Path, "mstsc.exe"), $"/v {_hostName}"));

            _loggedOnUsers = new ReactiveList<LoggedOnUserInfo>();

            _isUacOn = Observable.Merge(
                    this.WhenAnyValue(vm => vm.HostName).WhereNotNull().SelectMany(hostName => GetIsUacOn(hostName).Select(isUacOn => (bool?)isUacOn).CatchAndReturn(null)),
                    ToggleUac.Select(isUacOn => (bool?)isUacOn))
                .ObserveOnDispatcher()
                .ToProperty(this, vm => vm.IsUacOn);

            this.WhenActivated(d =>
            {
                this.WhenAnyValue(vm => vm.HostName)
                    .WhereNotNull()
                    .Select(hostName => ComputerObject.GetLoggedInUsers(hostName, TaskPoolScheduler.Default).Catch(Observable.Empty<LoggedOnUserInfo>()))
                    .Do(_ => _loggedOnUsers.Clear())
                    .Switch()
                    .ObserveOnDispatcher()
                    .Subscribe(userInfo => _loggedOnUsers.Add(userInfo))
                    .DisposeWith(d);

                Observable.Merge<(bool ShowLoggedOnUsers, bool ShowRemoteControlOptions)>(
                        this.WhenAnyValue(vm => vm.IsShowingLoggedOnUsers).Where(true).Select(_ => (true, false)),
                        this.WhenAnyValue(vm => vm.IsShowingRemoteControlOptions).Where(true).Select(_ => (false, true)))
                    .Subscribe(showSubView =>
                    {
                        IsShowingLoggedOnUsers = showSubView.ShowLoggedOnUsers;
                        IsShowingRemoteControlOptions = showSubView.ShowRemoteControlOptions;
                    })
                    .DisposeWith(d);

                Observable.Merge<(string Title, string Message)>(
                        OpenUser.ThrownExceptions.Select(ex => (("Could not open user", ex.Message))),
                        LogOffUser.ThrownExceptions.Select(ex => ("Could not log off user", ex.Message)),
                        StartRemoteControl.ThrownExceptions.Select(ex => (("Could not start remote control", ex.Message))),
                        StartRemoteControlClassic.ThrownExceptions.Select(ex => (("Could not start remote control", ex.Message))),
                        StartRemoteControl2012.ThrownExceptions.Select(ex => (("Could not start remote control", ex.Message))),
                        KillRemoteTools.ThrownExceptions.Select(ex => (("Could not kill remote tools", ex.Message))),
                        ToggleUac.ThrownExceptions.Select(ex => (("Could not disable UAC", ex.Message))),
                        StartRemoteAssistance.ThrownExceptions.Select(ex => (("Could not start remote assistance", ex.Message))),
                        StartRdp.ThrownExceptions.Select(ex => (("Could not start RDP", ex.Message))),
                        _isUacOn.ThrownExceptions.Select(ex => (("Could not get UAC status", ex.Message))))
                    .SelectMany(dialogContent => _messages.Handle(new MessageInfo(MessageType.Error, dialogContent.Message, dialogContent.Title)))
                    .Subscribe()
                    .DisposeWith(d);
            });
        }



        public ReactiveCommand<Unit, Unit> OpenUser { get; }
        public ReactiveCommand<Unit, Unit> CopyUserName { get; }
        public ReactiveCommand<Unit, Unit> LogOffUser { get; }
        public ReactiveCommand<Unit, Unit> StartRemoteControl { get; }
        public ReactiveCommand<Unit, Unit> StartRemoteControlClassic { get; }
        public ReactiveCommand<Unit, Unit> StartRemoteControl2012 { get; }
        public ReactiveCommand<Unit, Unit> KillRemoteTools { get; }
        public ReactiveCommand<Unit, bool> ToggleUac { get; }
        public ReactiveCommand<Unit, Unit> StartRemoteAssistance { get; }
        public ReactiveCommand<Unit, Unit> StartRdp { get; }
        public ReactiveList<LoggedOnUserInfo> LoggedOnUsers => _loggedOnUsers;
        public bool? IsUacOn => _isUacOn.Value;
        public string HostName { get => _hostName; set => this.RaiseAndSetIfChanged(ref _hostName, value); }
        public bool IsShowingLoggedOnUsers { get => _isShowingLoggedOnUsers; set => this.RaiseAndSetIfChanged(ref _isShowingLoggedOnUsers, value); }
        public LoggedOnUserInfo SelectedLoggedOnUser { get => _selectedLoggedOnUser; set => this.RaiseAndSetIfChanged(ref _selectedLoggedOnUser, value); }
        public bool IsShowingRemoteControlOptions { get => _isShowingRemoteControlOptions; set => this.RaiseAndSetIfChanged(ref _isShowingRemoteControlOptions, value); }



        private IObservable<Unit> LogOffUserImpl(int sessionId, string hostName) => Observable.Start(() =>
        {
            using (var powerShell = PowerShell.Create())
            {
                powerShell
                    .AddCommand("Invoke-Command")
                    .AddParameter("ScriptBlock", ScriptBlock.Create($"logoff {sessionId}"))
                    .AddParameter("ComputerName", hostName)
                .Invoke();
            }
        }, TaskPoolScheduler.Default);

        private IObservable<Unit> StartRemoteControlImpl(string hostName) => Observable.Start(() =>
        {
            EnsureComputerIsReachable(hostName);

            var keyHive = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, $"{hostName}", RegistryView.Registry64);
            var regKey = keyHive.OpenSubKey(@"SOFTWARE\Microsoft\SMS\Mobile Client", false);
            var sccmMajorVersion = int.Parse(regKey?.GetValue("ProductVersion").ToString().Substring(0, 1) ?? "0");

            if (sccmMajorVersion == 4) return StartRemoteControlClassicImpl(hostName);
            else return StartRemoteControl2012Impl(hostName);
        }, TaskPoolScheduler.Default)
        .SelectMany(action => action)
        .Catch(StartRemoteAssistanceImpl(hostName));

        private IObservable<Unit> StartRemoteControlClassicImpl(string hostName) => Observable.Defer(() => Observable.Start(
            () => RunFileFromCache("RemoteControl", "rc.exe", $"1 {hostName}"),
            TaskPoolScheduler.Default));

        private IObservable<Unit> StartRemoteControl2012Impl(string hostName) => Observable.Defer(() => Observable.Start(
            () => RunFileFromCache("RemoteControl2012", "CmRcViewer.exe", hostName),
            TaskPoolScheduler.Default));

        private IObservable<Unit> StartRemoteAssistanceImpl(string hostName) => Observable.Defer(() => Observable.Start(
            () => RunFile(Path.Combine(System32Path, "msra.exe"), $"/offerra {hostName}"),
            TaskPoolScheduler.Default));

        private IObservable<Unit> KillRemoteToolsImpl(string hostName) => Observable.Start(() =>
        {
            RunFile(Path.Combine(System32Path, "taskkill.exe"), $"/s {hostName} /im rc.exe /f", false);
            RunFile(Path.Combine(System32Path, "taskkill.exe"), $"/s {hostName} /im CmRcViewer.exe /f", false);
            RunFile(Path.Combine(System32Path, "taskkill.exe"), $"/s {hostName} /im msra.exe /f", false);
        }, TaskPoolScheduler.Default);

        private IObservable<bool> ToggleUacImpl(string hostName) => Observable.Start(() =>
        {
            var regValueName = "EnableLUA";
            var keyHive = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, hostName, RegistryView.Registry64);
            var key = keyHive.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", true);

            if (GetIsUacOn(hostName).Wait())
            {
                key?.SetValue(regValueName, 0);
                return false;
            }
            else
            {
                key?.SetValue(regValueName, 1);
                return true;
            }
        }, TaskPoolScheduler.Default).Concat(GetIsUacOn(hostName));



        private void EnsureComputerIsReachable(string hostName) { if (new Ping().Send(hostName, 1000)?.Status != IPStatus.Success) throw new Exception($"Could not reach {hostName}"); }

        private IObservable<bool> GetIsUacOn(string hostName) => Observable.Start(() =>
        {
            EnsureComputerIsReachable(hostName);

            var regValueName = "EnableLUA";
            var keyHive = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, hostName, RegistryView.Registry64);
            var key = keyHive.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System");
            if (int.TryParse(key?.GetValue(regValueName).ToString(), out int i))
            {
                return i == 1;
            }
            else
            {
                throw new Exception("Could not read registry value");
            }
        }, TaskPoolScheduler.Default);



        private readonly ReactiveList<LoggedOnUserInfo> _loggedOnUsers;
        private readonly ObservableAsPropertyHelper<bool?> _isUacOn;
        private string _hostName;
        private bool _isShowingLoggedOnUsers;
        private LoggedOnUserInfo _selectedLoggedOnUser;
        private bool _isShowingRemoteControlOptions;
    }
}
