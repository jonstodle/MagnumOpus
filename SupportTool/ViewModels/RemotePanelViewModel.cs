using Microsoft.Win32;
using ReactiveUI;
using SupportTool.Models;
using SupportTool.Services.NavigationServices;
using SupportTool.Services.SettingsServices;
using System;
using System.IO;
using System.Net.NetworkInformation;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using static SupportTool.Services.FileServices.ExecutionService;

namespace SupportTool.ViewModels
{
	public class RemotePanelViewModel : ViewModelBase
	{
		private readonly ReactiveCommand<Unit, Unit> _openUser;
		private readonly ReactiveCommand<Unit, Unit> _copyUserName;
		private readonly ReactiveCommand<Unit, Unit> _logOffUser;
		private readonly ReactiveCommand<Unit, Unit> _openLoggedOnUserDetails;
		private readonly ReactiveCommand<Unit, Unit> _startRemoteControl;
		private readonly ReactiveCommand<Unit, Unit> _startRemoteControlClassic;
		private readonly ReactiveCommand<Unit, Unit> _startRemoteControl2012;
		private readonly ReactiveCommand<Unit, Unit> _killRemoteTools;
		private readonly ReactiveCommand<Unit, bool> _toggleUac;
		private readonly ReactiveCommand<Unit, Unit> _startRemoteAssistance;
		private readonly ReactiveCommand<Unit, Unit> _startRdp;
		private readonly ReactiveList<LoggedOnUserInfo> _loggedOnUsers;
		private readonly ObservableAsPropertyHelper<bool?> _isUacOn;
		private ComputerObject _computer;
		private bool _isShowingLoggedOnUsers;
		private LoggedOnUserInfo _selectedLoggedOnUser;



		public RemotePanelViewModel()
		{
			_openUser = ReactiveCommand.CreateFromTask(async () => await NavigationService.ShowWindow<Views.UserWindow>(Tuple.Create(_selectedLoggedOnUser.Username, _computer.CN)));

			_copyUserName = ReactiveCommand.Create(() => Clipboard.SetText(_selectedLoggedOnUser.Username));

			_logOffUser = ReactiveCommand.Create(() => ExecuteFile(Path.Combine(System32Path, "logoff.exe"), $"{_selectedLoggedOnUser.SessionID} /server:{_computer.CN}", false));

			_openLoggedOnUserDetails = ReactiveCommand.Create(() => ExecuteCmd(Path.Combine(System32Path, "quser.exe"), $"/server:{_computer.CN}"));

			_startRemoteControl = ReactiveCommand.CreateFromObservable(() => StartRemoteControlImpl(_computer));

			_startRemoteControlClassic = ReactiveCommand.Create(() => StartRemoteControlClassicImpl(_computer));

			_startRemoteControl2012 = ReactiveCommand.Create(() => StartRemoteControl2012Impl(_computer));

			_killRemoteTools = ReactiveCommand.CreateFromObservable(() => KillRemoteToolsImpl(_computer));

			_toggleUac = ReactiveCommand.CreateFromObservable(() => ToggleUacImpl(_computer));

			_startRemoteAssistance = ReactiveCommand.Create(() => ExecuteFile(Path.Combine(System32Path, "msra.exe"), $"/offerra {_computer.CN}"));

			_startRdp = ReactiveCommand.Create(() => ExecuteFile(Path.Combine(System32Path, "mstsc.exe"), $"/v {_computer.CN}"));

			_loggedOnUsers = new ReactiveList<LoggedOnUserInfo>();

			_isUacOn = Observable.Merge(
				this.WhenAnyValue(x => x.Computer).WhereNotNull().SelectMany(x => GetIsUacOn(x.CN).Select(y => (bool?)y).Catch(Observable.Return<bool?>(null))),
				_toggleUac.Select(x => (bool?)x))
				.ObserveOnDispatcher()
				.ToProperty(this, x => x.IsUacOn);

            this.WhenActivated(disposables =>
            {
                this.WhenAnyValue(x => x.Computer)
                .WhereNotNull()
                .Select(x => x.GetLoggedInUsers().SubscribeOn(TaskPoolScheduler.Default).Catch(Observable.Return<LoggedOnUserInfo>(null)).WhereNotNull())
                .Do(_ => _loggedOnUsers.Clear())
                .Switch()
                .ObserveOnDispatcher()
                .Subscribe(x => _loggedOnUsers.Add(x))
                .DisposeWith(disposables);

                Observable.Merge(
                    _openUser.ThrownExceptions,
                    _openLoggedOnUserDetails.ThrownExceptions,
                    _startRemoteControl.ThrownExceptions,
                    _startRemoteControlClassic.ThrownExceptions,
                    _startRemoteControl2012.ThrownExceptions,
                    _killRemoteTools.ThrownExceptions,
                    _toggleUac.ThrownExceptions,
                    _startRemoteAssistance.ThrownExceptions,
                    _startRdp.ThrownExceptions,
                    _isUacOn.ThrownExceptions)
                    .Subscribe(async ex => await _errorMessages.Handle(new MessageInfo(ex.Message, "Could not launch external program")))
                    .DisposeWith(disposables);
            });
		}



		public ReactiveCommand OpenUser => _openUser;

		public ReactiveCommand CopyUserName => _copyUserName;

        public ReactiveCommand LogOffUser => _logOffUser;

		public ReactiveCommand OpenLoggedOnUserDetails => _openLoggedOnUserDetails;

		public ReactiveCommand StartRemoteControl => _startRemoteControl;

		public ReactiveCommand StartRemoteControlClassic => _startRemoteControlClassic;

		public ReactiveCommand StartRemoteControl2012 => _startRemoteControl2012;

		public ReactiveCommand KillRemoteTools => _killRemoteTools;

		public ReactiveCommand ToggleUac => _toggleUac;

		public ReactiveCommand StartRemoteAssistance => _startRemoteAssistance;

		public ReactiveCommand StartRdp => _startRdp;

		public ReactiveList<LoggedOnUserInfo> LoggedOnUsers => _loggedOnUsers;

		public bool? IsUacOn => _isUacOn.Value;

		public ComputerObject Computer
		{
			get { return _computer; }
			set { this.RaiseAndSetIfChanged(ref _computer, value); }
		}

		public bool IsShowingLoggedOnUsers
		{
			get { return _isShowingLoggedOnUsers; }
			set { this.RaiseAndSetIfChanged(ref _isShowingLoggedOnUsers, value); }
		}

		public LoggedOnUserInfo SelectedLoggedOnUser
		{
			get { return _selectedLoggedOnUser; }
			set { this.RaiseAndSetIfChanged(ref _selectedLoggedOnUser, value); }
		}



		private IObservable<Unit> StartRemoteControlImpl(ComputerObject computer) => Observable.Start(() =>
		{
			EnsureComputerIsReachable(computer.CN);

			var keyHive = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, $"{computer.CN}", RegistryView.Registry64);
			var regKey = keyHive.OpenSubKey(@"SOFTWARE\Microsoft\SMS\Mobile Client", false);
			var sccmMajorVersion = int.Parse(regKey.GetValue("ProductVersion").ToString().Substring(0, 1));

			if (sccmMajorVersion == 4) StartRemoteControlClassicImpl(computer);
			else StartRemoteControl2012Impl(computer);
		});

		private void StartRemoteControlClassicImpl(ComputerObject computer) => ExecuteFile(SettingsService.Current.RemoteControlClassicPath, $"1 {computer.CN}");

		private void StartRemoteControl2012Impl(ComputerObject computer) => ExecuteFile(SettingsService.Current.RemoteControl2012Path, computer.CN);

		private IObservable<Unit> KillRemoteToolsImpl(ComputerObject computer) => Observable.Start(() =>
		{
			ExecuteFile(Path.Combine(System32Path, "taskkill.exe"), $"/s {_computer.CN} /im rcagent.exe /f", false);
			ExecuteFile(Path.Combine(System32Path, "taskkill.exe"), $"/s {_computer.CN} /im CmRcService.exe /f", false);
			ExecuteFile(Path.Combine(System32Path, "taskkill.exe"), $"/s {_computer.CN} /im msra.exe /f", false);
		});

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
		}).Concat(GetIsUacOn(computer.CN));



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
		});
	}
}
