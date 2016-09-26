using ReactiveUI;
using SupportTool.Services.DialogServices;
using System;
using System.Diagnostics;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;

namespace SupportTool.ViewModels
{
	public class IPAddressPanelViewModel : ReactiveObject
	{
		private readonly ReactiveCommand<Unit, Unit> _openLoggedOn;
		private readonly ReactiveCommand<Unit, Unit> _openLoggedOnPlus;
		private readonly ReactiveCommand<Unit, Unit> _openRemoteExecution;
		private readonly ReactiveCommand<Unit, Unit> _openCDrive;
		private readonly ReactiveCommand<Unit, Unit> _rebootComputer;
		private readonly ReactiveCommand<Unit, Unit> _startRemoteControl;
		private readonly ReactiveCommand<Unit, Unit> _startRdp;
		private string _ipAddress;



		public IPAddressPanelViewModel()
		{
			_openLoggedOn = ReactiveCommand.Create(() => ExecuteCmd(@"C:\PsTools\PsLoggedon.exe", $@"\\{_ipAddress}"));

			_openLoggedOnPlus = ReactiveCommand.Create(() => ExecuteCmd(@"C:\PsTools\PsExec.exe", $@"\\{_ipAddress} C:\Windows\System32\cmd.exe /K query user"));

			_openRemoteExecution = ReactiveCommand.Create(() => ExecuteCmd(@"C:\PsTools\PsExec.exe", $@"\\{_ipAddress} C:\Windows\System32\cmd.exe"));

			_openCDrive = ReactiveCommand.Create(() => { Process.Start($@"\\{_ipAddress}\C$"); });
			_openCDrive
				.ThrownExceptions
				.Subscribe(ex => DialogService.ShowError(ex.Message, "Could not open location"));

			_rebootComputer = ReactiveCommand.Create(() =>
			{
				if (DialogService.ShowPrompt($"Reboot {_ipAddress}?"))
				{
					ExecuteFile(@"C:\Windows\System32\shutdown.exe", $@"-r -f -m \\{_ipAddress} -t 0");
				}
			});

			_startRemoteControl = ReactiveCommand.Create(() => StartRemoteControlImpl(_ipAddress));

			_startRdp = ReactiveCommand.Create(() => ExecuteFile(@"C:\Windows\System32\mstsc.exe", $"/v {_ipAddress}"));

			Observable.Merge(
				_openLoggedOn.ThrownExceptions,
				_openRemoteExecution.ThrownExceptions,
				_startRemoteControl.ThrownExceptions,
				_startRdp.ThrownExceptions)
				.Subscribe(ex => DialogService.ShowError(ex.Message, "Could not launch external program"));
		}



		public ReactiveCommand OpenLoggedOn => _openLoggedOn;

		public ReactiveCommand OpenLoggedOnPlus => _openLoggedOnPlus;

		public ReactiveCommand OpenRemoteExecution => _openRemoteExecution;

		public ReactiveCommand OpenCDrive => _openCDrive;

		public ReactiveCommand RebootComputer => _rebootComputer;

		public ReactiveCommand StartRemoteControl => _startRemoteControl;

		public ReactiveCommand StartRdp => _startRdp;

		public string IPAddress
		{
			get { return _ipAddress; }
			set { this.RaiseAndSetIfChanged(ref _ipAddress, value); }
		}



		private void ExecuteCmd(string fileName, string arguments = "") => ExecuteFile(@"C:\Windows\System32\cmd.exe", $@"/K {fileName} {arguments}");

		private void ExecuteFile(string fileName, string arguments = "")
		{
			if (File.Exists(fileName)) Process.Start(fileName, arguments);
			else throw new ArgumentException($"Could not find {fileName}");
		}

		private void StartRemoteControlImpl(string ipAddress)
		{
			var fileName = @"C:\SCCM Remote Control\rc.exe";
			var arguments = $"1 {ipAddress}";

			//if (computer.Company == "SIHF"
			//	|| computer.Company == "REV"
			//	|| computer.Company == "SOHF")
			//{
			//	fileName = @"C:\RemoteControl2012\CmRcViewer.exe";
			//	arguments = computer.CN;
			//}

			ExecuteFile(fileName, arguments);
		}
	}
}
