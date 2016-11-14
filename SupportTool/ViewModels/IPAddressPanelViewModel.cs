using ReactiveUI;
using SupportTool.Models;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reactive;
using System.Reactive.Linq;

using static SupportTool.Executables.Helpers;

namespace SupportTool.ViewModels
{
	public class IPAddressPanelViewModel : ViewModelBase
	{
		private readonly ReactiveCommand<Unit, Unit> _openLoggedOn;
		private readonly ReactiveCommand<Unit, Unit> _openLoggedOnPlus;
		private readonly ReactiveCommand<Unit, Unit> _openRemoteExecution;
		private readonly ReactiveCommand<Unit, Unit> _openCDrive;
		private readonly ReactiveCommand<Unit, Unit> _rebootComputer;
		private readonly ReactiveCommand<Unit, Unit> _startRemoteControl;
		private readonly ReactiveCommand<Unit, Unit> _startRemoteControl2012;
		private readonly ReactiveCommand<Unit, Unit> _killRemoteControl;
		private readonly ReactiveCommand<Unit, Unit> _startRemoteAssistance;
		private readonly ReactiveCommand<Unit, Unit> _startRdp;
		private readonly ObservableAsPropertyHelper<string> _hostName;
		private string _ipAddress;



		public IPAddressPanelViewModel()
		{
			_openLoggedOn = ReactiveCommand.Create(() =>
			{
				EnsureExecutableIsAvailable("PsLoggedon.exe");
				ExecuteCmd(Path.Combine(Directory.GetCurrentDirectory(), "PsLoggedon.exe"), $@"\\{_ipAddress}");
			});

			_openLoggedOnPlus = ReactiveCommand.Create(() =>
			{
				EnsureExecutableIsAvailable("PsExec.exe");
				ExecuteCmd(Path.Combine(Directory.GetCurrentDirectory(), "PsExec.exe"), $@"\\{_ipAddress} C:\Windows\System32\cmd.exe /K query user");
			});

			_openRemoteExecution = ReactiveCommand.Create(() =>
			{
				EnsureExecutableIsAvailable("PsExec.exe");
				ExecuteCmd(Path.Combine(Directory.GetCurrentDirectory(), "PsExec.exe"), $@"\\{_ipAddress} C:\Windows\System32\cmd.exe");
			});

			_openCDrive = ReactiveCommand.Create(() => { Process.Start($@"\\{_ipAddress}\C$"); });
			_openCDrive
				.ThrownExceptions
				.Subscribe(async ex => await _errorMessages.Handle(new MessageInfo(ex.Message, "Could not open location")));

			_rebootComputer = ReactiveCommand.Create(() =>
			{
				if (_promptMessages.Handle(new MessageInfo($"Reboot {_ipAddress}?", "", "Yes", "No")).Wait() == 0)
				{
					ExecuteFile(@"C:\Windows\System32\shutdown.exe", $@"-r -f -m \\{_ipAddress} -t 0");
				}
			});

			_startRemoteControl = ReactiveCommand.Create(() => ExecuteFile(@"C:\SCCM Remote Control\rc.exe", $"1 {_ipAddress}"));

			_startRemoteControl2012 = ReactiveCommand.Create(() => ExecuteFile(@"C:\RemoteControl2012\CmRcViewer.exe", _ipAddress));

			_killRemoteControl = ReactiveCommand.Create(() => ExecuteFile(@"C:\Windows\System32\taskkill.exe", $"/s {_ipAddress} /im rcagent.exe /f"));

			_startRemoteAssistance = ReactiveCommand.Create(() => ExecuteFile(@"C:\Windows\System32\msra.exe", $"/offerra {_ipAddress}"));

			_startRdp = ReactiveCommand.Create(() => ExecuteFile(@"C:\Windows\System32\mstsc.exe", $"/v {_ipAddress}"));

			_hostName = this.WhenAnyValue(x => x.IPAddress)
				.Where(x => x.HasValue())
				.Select(x => Dns.GetHostEntry(x).HostName)
				.Catch(Observable.Return(""))
				.ToProperty(this, x => x.HostName, null);

			Observable.Merge(
				_openLoggedOn.ThrownExceptions,
				_openRemoteExecution.ThrownExceptions,
				_startRemoteControl.ThrownExceptions,
				_startRdp.ThrownExceptions)
				.Subscribe(async ex => await _errorMessages.Handle(new MessageInfo(ex.Message, "Could not launch external program")));
		}



		public ReactiveCommand OpenLoggedOn => _openLoggedOn;

		public ReactiveCommand OpenLoggedOnPlus => _openLoggedOnPlus;

		public ReactiveCommand OpenRemoteExecution => _openRemoteExecution;

		public ReactiveCommand OpenCDrive => _openCDrive;

		public ReactiveCommand RebootComputer => _rebootComputer;

		public ReactiveCommand StartRemoteControl => _startRemoteControl;

		public ReactiveCommand StartRemoteControl2012 => _startRemoteControl2012;

		public ReactiveCommand KillRemoteControl => _killRemoteControl;

		public ReactiveCommand StartRemoteAssistance => _startRemoteAssistance;

		public ReactiveCommand StartRdp => _startRdp;

		public string HostName => _hostName.Value;

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
	}
}
