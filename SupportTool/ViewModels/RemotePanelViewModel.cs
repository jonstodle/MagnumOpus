using ReactiveUI;
using SupportTool.Models;
using SupportTool.Services.DialogServices;
using System;
using System.Diagnostics;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;

namespace SupportTool.ViewModels
{
	public class RemotePanelViewModel : ReactiveObject
	{
		private readonly ReactiveCommand<Unit, Unit> _openLoggedOn;
		private readonly ReactiveCommand<Unit, Unit> _openLoggedOnPlus;
		private readonly ReactiveCommand<Unit, Unit> _openRemoteExecution;
		private readonly ReactiveCommand<Unit, Unit> _openCDrive;
		private readonly ReactiveCommand<Unit, Unit> _rebootComputer;
		private readonly ReactiveCommand<Unit, Unit> _startRemoteControl;
		private readonly ReactiveCommand<Unit, Unit> _killRemoteControl;
		private readonly ReactiveCommand<Unit, Unit> _startRemoteAssistance;
		private readonly ReactiveCommand<Unit, Unit> _startRdp;
		private ComputerObject _computer;



		public RemotePanelViewModel()
		{
			_openLoggedOn = ReactiveCommand.Create(() => ExecuteCmd(@"C:\PsTools\PsLoggedon.exe", $@"\\{_computer.CN}"));

			_openLoggedOnPlus = ReactiveCommand.Create(() => ExecuteCmd(@"C:\PsTools\PsExec.exe", $@"\\{_computer.CN} C:\Windows\System32\cmd.exe /K query user"));

			_openRemoteExecution = ReactiveCommand.Create(() => ExecuteCmd(@"C:\PsTools\PsExec.exe", $@"\\{_computer.CN} C:\Windows\System32\cmd.exe"));

			_openCDrive = ReactiveCommand.Create(() => { Process.Start($@"\\{_computer.CN}\C$"); });
			_openCDrive
				.ThrownExceptions
				.Subscribe(ex => DialogService.ShowError(ex.Message, "Could not open location"));

			_rebootComputer = ReactiveCommand.Create(() =>
			{
				if (DialogService.ShowPrompt($"Reboot {_computer.CN}?"))
				{
					ExecuteFile(@"C:\Windows\System32\shutdown.exe", $@"-r -f -m \\{_computer.CN} -t 0");
				}
			});

			_startRemoteControl = ReactiveCommand.Create(() => StartRemoteControlImpl(_computer));

			_killRemoteControl = ReactiveCommand.Create(() => ExecuteFile(@"C:\Windows\System32\taskkill.exe", $"/s {_computer.CN} /im rcagent.exe /f"));

			_startRemoteAssistance = ReactiveCommand.Create(() => ExecuteFile(@"C:\Windows\System32\msra.exe", $"/offerra {_computer.CN}"));

			_startRdp = ReactiveCommand.Create(() => ExecuteFile(@"C:\Windows\System32\mstsc.exe", $"/v {_computer.CN}"));

			Observable.Merge(
				_openLoggedOn.ThrownExceptions,
				_openRemoteExecution.ThrownExceptions,
				_startRemoteControl.ThrownExceptions,
				_killRemoteControl.ThrownExceptions,
				_startRemoteAssistance.ThrownExceptions,
				_startRdp.ThrownExceptions)
				.Subscribe(ex => DialogService.ShowError(ex.Message, "Could not launch external program"));
		}



		public ReactiveCommand OpenLoggedOn => _openLoggedOn;

		public ReactiveCommand OpenLoggedOnPlus => _openLoggedOnPlus;

		public ReactiveCommand OpenRemoteExecution => _openRemoteExecution;

		public ReactiveCommand OpenCDrive => _openCDrive;

		public ReactiveCommand RebootComputer => _rebootComputer;

		public ReactiveCommand StartRemoteControl => _startRemoteControl;

		public ReactiveCommand KillRemoteControl => _killRemoteControl;

		public ReactiveCommand StartRemoteAssistance => _startRemoteAssistance;

		public ReactiveCommand StartRdp => _startRdp;

		public ComputerObject Computer
		{
			get { return _computer; }
			set { this.RaiseAndSetIfChanged(ref _computer, value); }
		}



		private void ExecuteCmd(string fileName, string arguments = "") => ExecuteFile(@"C:\Windows\System32\cmd.exe", $@"/K {fileName} {arguments}");

		private void ExecuteFile(string fileName, string arguments = "")
		{
			if (File.Exists(fileName)) Process.Start(fileName, arguments);
			else throw new ArgumentException($"Could not find {fileName}");
		}

		private void StartRemoteControlImpl(ComputerObject computer)
		{
			var fileName = @"C:\SCCM Remote Control\rc.exe";
			var arguments = $"1 {computer.CN}";

			if (computer.Company == "SIHF"
				|| computer.Company == "REV"
				|| computer.Company == "SOHF")
			{
				fileName = @"C:\RemoteControl2012\CmRcViewer.exe";
				arguments = computer.CN;
			}

			ExecuteFile(fileName, arguments);
		}
	}
}
