﻿using Microsoft.Win32;
using ReactiveUI;
using MagnumOpus.Models;
using MagnumOpus.Services.NavigationServices;
using System;
using System.IO;
using System.Net.NetworkInformation;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using static MagnumOpus.Services.FileServices.ExecutionService;
using System.Management.Automation;

namespace MagnumOpus.ViewModels
{
    public class RemotePanelViewModel : ViewModelBase
	{
		public RemotePanelViewModel()
		{
			OpenUser = ReactiveCommand.CreateFromObservable(() => Observable.FromAsync(() => NavigationService.ShowWindow<Views.UserWindow>(Tuple.Create(_selectedLoggedOnUser.Username, _computer.CN))));

			CopyUserName = ReactiveCommand.Create(() => Clipboard.SetText(_selectedLoggedOnUser.Username));

			LogOffUser = ReactiveCommand.CreateFromObservable(() => LogOffUserImpl(_selectedLoggedOnUser.SessionID, Computer.CN));

			StartRemoteControl = ReactiveCommand.CreateFromObservable(() => StartRemoteControlImpl(_computer.CN));

			StartRemoteControlClassic = ReactiveCommand.CreateFromObservable(() => StartRemoteControlClassicImpl(_computer.CN));

			StartRemoteControl2012 = ReactiveCommand.CreateFromObservable(() => StartRemoteControl2012Impl(_computer.CN));

            StartRemoteAssistance = ReactiveCommand.CreateFromObservable(() => StartRemoteAssistanceImpl(_computer.CN));

            KillRemoteTools = ReactiveCommand.CreateFromObservable(() => KillRemoteToolsImpl(_computer.CN));

			ToggleUac = ReactiveCommand.CreateFromObservable(() => ToggleUacImpl(_computer.CN));

			StartRdp = ReactiveCommand.Create(() => RunFile(Path.Combine(System32Path, "mstsc.exe"), $"/v {_computer.CN}"));

			_loggedOnUsers = new ReactiveList<LoggedOnUserInfo>();

			_isUacOn = Observable.Merge(
				    this.WhenAnyValue(vm => vm.Computer).WhereNotNull().SelectMany(computer => GetIsUacOn(computer.CN).Select(isUacOn => (bool?)isUacOn).CatchAndReturn(null)),
				    ToggleUac.Select(isUacOn => (bool?)isUacOn))
				.ObserveOnDispatcher()
				.ToProperty(this, vm => vm.IsUacOn);

            this.WhenActivated(disposables =>
            {
                this.WhenAnyValue(vm => vm.Computer)
                    .WhereNotNull()
                    .Select(computer => computer.GetLoggedInUsers(TaskPoolScheduler.Default).Catch(Observable.Empty<LoggedOnUserInfo>()))
                    .Do(_ => _loggedOnUsers.Clear())
                    .Switch()
                    .ObserveOnDispatcher()
                    .Subscribe(userInfo => _loggedOnUsers.Add(userInfo))
                    .DisposeWith(disposables);

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
                    .DisposeWith(disposables);
            });
		}



		public ReactiveCommand<Unit, Unit> OpenUser { get; private set; }
		public ReactiveCommand<Unit, Unit> CopyUserName { get; private set; }
        public ReactiveCommand<Unit, Unit> LogOffUser { get; private set; }
        public ReactiveCommand<Unit, Unit> StartRemoteControl { get; private set; }
        public ReactiveCommand<Unit, Unit> StartRemoteControlClassic { get; private set; }
        public ReactiveCommand<Unit, Unit> StartRemoteControl2012 { get; private set; }
        public ReactiveCommand<Unit, Unit> KillRemoteTools { get; private set; }
        public ReactiveCommand<Unit, bool> ToggleUac { get; private set; }
        public ReactiveCommand<Unit, Unit> StartRemoteAssistance { get; private set; }
        public ReactiveCommand<Unit, Unit> StartRdp { get; private set; }
        public ReactiveList<LoggedOnUserInfo> LoggedOnUsers => _loggedOnUsers;
		public bool? IsUacOn => _isUacOn.Value;
        public ComputerObject Computer { get => _computer; set => this.RaiseAndSetIfChanged(ref _computer, value); }
        public bool IsShowingLoggedOnUsers { get => _isShowingLoggedOnUsers; set => this.RaiseAndSetIfChanged(ref _isShowingLoggedOnUsers, value); }
        public LoggedOnUserInfo SelectedLoggedOnUser { get => _selectedLoggedOnUser; set => this.RaiseAndSetIfChanged(ref _selectedLoggedOnUser, value); }



        private IObservable<Unit> LogOffUserImpl(int sessionId, string computerCn) => Observable.Start(() =>
        {
            using (var powerShell = PowerShell.Create())
            {
                powerShell
                    .AddCommand("Invoke-Command")
                    .AddParameter("ScriptBlock", ScriptBlock.Create($"logoff {sessionId}"))
                    .AddParameter("ComputerName", computerCn)
                .Invoke();
            }
        }, TaskPoolScheduler.Default);

        private IObservable<Unit> StartRemoteControlImpl(string computerCn) => Observable.Start(() =>
		{
			EnsureComputerIsReachable(computerCn);

			var keyHive = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, $"{computerCn}", RegistryView.Registry64);
			var regKey = keyHive.OpenSubKey(@"SOFTWARE\Microsoft\SMS\Mobile Client", false);
			var sccmMajorVersion = int.Parse(regKey.GetValue("ProductVersion").ToString().Substring(0, 1));

			if (sccmMajorVersion == 4) StartRemoteControlClassicImpl(computerCn);
			else StartRemoteControl2012Impl(computerCn);
		}, TaskPoolScheduler.Default)
        .Catch(StartRemoteAssistanceImpl(computerCn));

        private IObservable<Unit> StartRemoteControlClassicImpl(string computerCn) => Observable.Start(
            () => RunFileFromCache( "RemoteControl", "rc.exe", $"1 {computerCn}"),
            TaskPoolScheduler.Default);

        private IObservable<Unit> StartRemoteControl2012Impl(string computerCn) => Observable.Start(
            () => RunFileFromCache("RemoteControl2012", "CmRcViewer.exe", computerCn),
            TaskPoolScheduler.Default);

        private IObservable<Unit> StartRemoteAssistanceImpl(string computerCn) => Observable.Start(
            () => RunFile(Path.Combine(System32Path, "msra.exe"), $"/offerra {_computer.CN}"),
            TaskPoolScheduler.Default);

		private IObservable<Unit> KillRemoteToolsImpl(string computerCn) => Observable.Start(() =>
		{
			RunFile(Path.Combine(System32Path, "taskkill.exe"), $"/s {computerCn} /im rc.exe /f", false);
			RunFile(Path.Combine(System32Path, "taskkill.exe"), $"/s {computerCn} /im CmRcViewer.exe /f", false);
			RunFile(Path.Combine(System32Path, "taskkill.exe"), $"/s {computerCn} /im msra.exe /f", false);
		}, TaskPoolScheduler.Default);

		private IObservable<bool> ToggleUacImpl(string computerCn) => Observable.Start(() =>
		{
			var regValueName = "EnableLUA";
			var keyHive = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, computerCn, RegistryView.Registry64);
			var key = keyHive?.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", true);

			if (GetIsUacOn(computerCn).Wait())
			{
				key.SetValue(regValueName, 0);
				return false;
			}
			else
			{
				key.SetValue(regValueName, 1);
				return true;
			}
		}, TaskPoolScheduler.Default).Concat(GetIsUacOn(computerCn));



		private void EnsureComputerIsReachable(string hostName) {if(new Ping().Send(hostName, 1000).Status != IPStatus.Success) throw new Exception($"Could not reach {hostName}"); }

		private IObservable<bool> GetIsUacOn(string hostName) => Observable.Start(() =>
		{
			EnsureComputerIsReachable(hostName);

			var regValueName = "EnableLUA";
			var keyHive = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, hostName, RegistryView.Registry64);
			var key = keyHive?.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System");
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
        private ComputerObject _computer;
        private bool _isShowingLoggedOnUsers;
        private LoggedOnUserInfo _selectedLoggedOnUser;
    }
}
