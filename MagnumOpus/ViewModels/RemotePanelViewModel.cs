using Microsoft.Win32;
using ReactiveUI;
using MagnumOpus.Models;
using MagnumOpus.Services.NavigationServices;
using MagnumOpus.Services.SettingsServices;
using System;
using System.IO;
using System.Net.NetworkInformation;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using static MagnumOpus.Services.FileServices.ExecutionService;
using MagnumOpus.Services.FileServices;

namespace MagnumOpus.ViewModels
{
	public class RemotePanelViewModel : ViewModelBase
	{
		public RemotePanelViewModel()
		{
			OpenUser = ReactiveCommand.CreateFromObservable(() => Observable.FromAsync(() => NavigationService.ShowWindow<Views.UserWindow>(Tuple.Create(_selectedLoggedOnUser.Username, _computer.CN))));

			CopyUserName = ReactiveCommand.Create(() => Clipboard.SetText(_selectedLoggedOnUser.Username));

			LogOffUser = ReactiveCommand.Create(() => RunFile(Path.Combine(System32Path, "logoff.exe"), $"{_selectedLoggedOnUser.SessionID} /server:{_computer.CN}", false));

			StartRemoteControl = ReactiveCommand.CreateFromObservable(() => StartRemoteControlImpl(_computer));

			StartRemoteControlClassic = ReactiveCommand.CreateFromObservable(() => StartRemoteControlClassicImpl(_computer));

			StartRemoteControl2012 = ReactiveCommand.CreateFromObservable(() => StartRemoteControl2012Impl(_computer));

			KillRemoteTools = ReactiveCommand.CreateFromObservable(() => KillRemoteToolsImpl(_computer));

			ToggleUac = ReactiveCommand.CreateFromObservable(() => ToggleUacImpl(_computer));

			StartRemoteAssistance = ReactiveCommand.Create(() => RunFile(Path.Combine(System32Path, "msra.exe"), $"/offerra {_computer.CN}"));

			StartRdp = ReactiveCommand.Create(() => RunFile(Path.Combine(System32Path, "mstsc.exe"), $"/v {_computer.CN}"));

			_loggedOnUsers = new ReactiveList<LoggedOnUserInfo>();

			_isUacOn = Observable.Merge(
				this.WhenAnyValue(x => x.Computer).WhereNotNull().SelectMany(x => GetIsUacOn(x.CN).Select(y => (bool?)y).CatchAndReturn(null)),
				ToggleUac.Select(x => (bool?)x))
				.ObserveOnDispatcher()
				.ToProperty(this, x => x.IsUacOn);

            (this).WhenActivated((Action<CompositeDisposable>)(disposables =>
            {
                (this).WhenAnyValue(x => x.Computer)
                .WhereNotNull()
                .Select(x => x.GetLoggedInUsers(TaskPoolScheduler.Default).CatchAndReturn(null).WhereNotNull())
                .Do((IObservable<LoggedOnUserInfo> _) => _loggedOnUsers.Clear())
                .Switch()
                .ObserveOnDispatcher()
                .Subscribe(x => _loggedOnUsers.Add(x))
                .DisposeWith(disposables);

                Observable.Merge(
                    Observable.Select<Exception, (string, string)>(this.OpenUser.ThrownExceptions, (Func<Exception, (string, string)>)(ex => ((string, string))(((string)"Could not open user", (string)ex.Message)))),
                    StartRemoteControl.ThrownExceptions.Select(ex => (("Could not start remote control", ex.Message))),
                    StartRemoteControlClassic.ThrownExceptions.Select(ex => (("Could not start remote control", ex.Message))),
                    StartRemoteControl2012.ThrownExceptions.Select(ex => (("Could not start remote control", ex.Message))),
                    KillRemoteTools.ThrownExceptions.Select(ex => (("Could not kill remote tools", ex.Message))),
                    ToggleUac.ThrownExceptions.Select(ex => (("Could not disable UAC", ex.Message))),
                    StartRemoteAssistance.ThrownExceptions.Select(ex => (("Could not start remote assistance", ex.Message))),
                    StartRdp.ThrownExceptions.Select(ex => (("Could not start RDP", ex.Message))),
                    _isUacOn.ThrownExceptions.Select(ex => (("Could not get UAC status", ex.Message))))
                    .SelectMany(((string, string) x) => _messages.Handle(new MessageInfo(MessageType.Error, x.Item2, x.Item1)))
                    .Subscribe()
                    .DisposeWith(disposables);
            }));
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



        private IObservable<Unit> StartRemoteControlImpl(ComputerObject computer) => Observable.Start(() =>
		{
			EnsureComputerIsReachable(computer.CN);

			var keyHive = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, $"{computer.CN}", RegistryView.Registry64);
			var regKey = keyHive.OpenSubKey(@"SOFTWARE\Microsoft\SMS\Mobile Client", false);
			var sccmMajorVersion = int.Parse(regKey.GetValue("ProductVersion").ToString().Substring(0, 1));

			if (sccmMajorVersion == 4) StartRemoteControlClassicImpl(computer);
			else StartRemoteControl2012Impl(computer);
		}, TaskPoolScheduler.Default);

        private IObservable<Unit> StartRemoteControlClassicImpl(ComputerObject computer) => Observable.Start(() =>
        {
            Executables.Helpers.WriteApplicationFilesToDisk("RemoteControl");
            RunFile(Path.Combine(FileService.LocalAppData, "RemoteControl", "rc.exe"), $"1 {computer.CN}");
        }, TaskPoolScheduler.Default);

        private IObservable<Unit> StartRemoteControl2012Impl(ComputerObject computer) => Observable.Start(() =>
        {
            Executables.Helpers.WriteApplicationFilesToDisk("RemoteControl2012");
            RunFile(Path.Combine(FileService.LocalAppData, "RemoteControl2012", "CmRcViewer.exe"), computer.CN);
        }, TaskPoolScheduler.Default);

		private IObservable<Unit> KillRemoteToolsImpl(ComputerObject computer) => Observable.Start(() =>
		{
			RunFile(Path.Combine(System32Path, "taskkill.exe"), $"/s {_computer.CN} /im rcagent.exe /f", false);
			RunFile(Path.Combine(System32Path, "taskkill.exe"), $"/s {_computer.CN} /im CmRcService.exe /f", false);
			RunFile(Path.Combine(System32Path, "taskkill.exe"), $"/s {_computer.CN} /im msra.exe /f", false);
		}, TaskPoolScheduler.Default);

		private IObservable<bool> ToggleUacImpl(ComputerObject computer) => Observable.Start(() =>
		{
			var regValueName = "EnableLUA";
			var keyHive = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, computer.CN, RegistryView.Registry64);
			var key = keyHive?.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", true);

			if (GetIsUacOn(computer.CN).Wait())
			{
				key.SetValue(regValueName, 0);
				return false;
			}
			else
			{
				key.SetValue(regValueName, 1);
				return true;
			}
		}, TaskPoolScheduler.Default).Concat(GetIsUacOn(computer.CN));



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
