using Microsoft.Win32;
using ReactiveUI;
using SupportTool.Models;
using SupportTool.Services.FileServices;
using SupportTool.Services.SettingsServices;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reactive;
using System.Reactive.Linq;

using static SupportTool.Executables.Helpers;
using static SupportTool.Services.FileServices.ExecutionService;

namespace SupportTool.ViewModels
{
	public class RemotePanelViewModel : ViewModelBase
	{
		private readonly ReactiveCommand<Unit, Unit> _openLoggedOn;
		private readonly ReactiveCommand<Unit, Unit> _openLoggedOnPlus;
		private readonly ReactiveCommand<Unit, Unit> _startRemoteControl;
		private readonly ReactiveCommand<Unit, Unit> _startRemoteControlClassic;
		private readonly ReactiveCommand<Unit, Unit> _startRemoteControl2012;
		private readonly ReactiveCommand<Unit, Unit> _killRemoteTools;
		private readonly ReactiveCommand<Unit, Unit> _startRemoteAssistance;
		private readonly ReactiveCommand<Unit, Unit> _startRdp;
		private ComputerObject _computer;



		public RemotePanelViewModel()
		{
			_openLoggedOn = ReactiveCommand.Create(() =>
			{
				EnsureExecutableIsAvailable("PsLoggedon.exe");
				ExecuteCmd($"\"{Path.Combine(FileService.LocalAppData, "PsLoggedon.exe")}\"", $@"\\{_computer.CN}");
			});

			_openLoggedOnPlus = ReactiveCommand.Create(() =>
			{
				EnsureExecutableIsAvailable("PsExec.exe");
				ExecuteCmd($"\"{Path.Combine(FileService.LocalAppData, "PsExec.exe")}\"", $@"\\{_computer.CN} C:\Windows\System32\cmd.exe /K query user");
			});

			_startRemoteControl = ReactiveCommand.CreateFromObservable(() => StartRemoteControlImpl(_computer));

			_startRemoteControlClassic = ReactiveCommand.Create(() => StartRemoteControlClassicImpl(_computer));

			_startRemoteControl2012 = ReactiveCommand.Create(() => StartRemoteControl2012Impl(_computer));

			_killRemoteTools = ReactiveCommand.CreateFromObservable(() => KillRemoteToolsImpl(_computer));

			_startRemoteAssistance = ReactiveCommand.Create(() => ExecuteFile(@"C:\Windows\System32\msra.exe", $"/offerra {_computer.CN}"));

			_startRdp = ReactiveCommand.Create(() => ExecuteFile(@"C:\Windows\System32\mstsc.exe", $"/v {_computer.CN}"));

			Observable.Merge(
				_openLoggedOn.ThrownExceptions,
				_startRemoteControl.ThrownExceptions,
				_startRemoteControlClassic.ThrownExceptions,
				_startRemoteControl2012.ThrownExceptions,
				_killRemoteTools.ThrownExceptions,
				_startRemoteAssistance.ThrownExceptions,
				_startRdp.ThrownExceptions)
				.Subscribe(async ex => await _errorMessages.Handle(new MessageInfo(ex.Message, "Could not launch external program")));
		}



		public ReactiveCommand OpenLoggedOn => _openLoggedOn;

		public ReactiveCommand OpenLoggedOnPlus => _openLoggedOnPlus;

		public ReactiveCommand StartRemoteControl => _startRemoteControl;

		public ReactiveCommand StartRemoteControlClassic => _startRemoteControlClassic;

		public ReactiveCommand StartRemoteControl2012 => _startRemoteControl2012;

		public ReactiveCommand KillRemoteTools => _killRemoteTools;

		public ReactiveCommand StartRemoteAssistance => _startRemoteAssistance;

		public ReactiveCommand StartRdp => _startRdp;

		public ComputerObject Computer
		{
			get { return _computer; }
			set { this.RaiseAndSetIfChanged(ref _computer, value); }
		}



		private IObservable<Unit> StartRemoteControlImpl(ComputerObject computer) => Observable.Start(() =>
		{
			var pingResult = new Ping().Send(computer.CN, 1000).Status;
			if (pingResult != IPStatus.Success) throw new Exception("Could not reach computer");

			var keyHive = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, $"{computer.CN}", RegistryView.Registry64);
			var regKey = keyHive.OpenSubKey(@"SOFTWARE\Microsoft\SMS\Mobile Client", false);
			var sccmMajorVersion = int.Parse(regKey.GetValue("ProductVersion").ToString().Substring(0, 1));

			if (sccmMajorVersion == 4) StartRemoteControlClassicImpl(computer);
			else StartRemoteControl2012Impl(computer);
		});

		private void StartRemoteControlClassicImpl(ComputerObject computer) => ExecuteFile(@"C:\SCCM Remote Control\rc.exe", $"1 {computer.CN}");

		private void StartRemoteControl2012Impl(ComputerObject computer) => ExecuteFile(@"C:\RemoteControl2012\CmRcViewer.exe", computer.CN);

		private IObservable<Unit> KillRemoteToolsImpl(ComputerObject computer) => Observable.Start(() =>
		{
			ExecuteFile(@"C:\Windows\System32\taskkill.exe", $"/s {_computer.CN} /im rcagent.exe /f");
			ExecuteFile(@"C:\Windows\System32\taskkill.exe", $"/s {_computer.CN} /im CmRcService.exe /f");
			ExecuteFile(@"C:\Windows\System32\taskkill.exe", $"/s {_computer.CN} /im msra.exe /f");
		});
	}
}
