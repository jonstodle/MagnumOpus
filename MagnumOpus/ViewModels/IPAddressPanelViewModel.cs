using ReactiveUI;
using MagnumOpus.Models;
using MagnumOpus.Services.FileServices;
using MagnumOpus.Services.SettingsServices;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;

using static MagnumOpus.Executables.Helpers;

namespace MagnumOpus.ViewModels
{
	public class IPAddressPanelViewModel : ViewModelBase
	{
		public IPAddressPanelViewModel()
		{
			OpenLoggedOn = ReactiveCommand.Create(() =>
			{
				//EnsureFileIsAvailable("PsLoggedon.exe");
				//ExecuteCmd(Path.Combine(Directory.GetCurrentDirectory(), "PsLoggedon.exe"), $@"\\{_ipAddress}");
			});

			OpenLoggedOnPlus = ReactiveCommand.Create(() =>
			{
				//EnsureFileIsAvailable("PsExec.exe");
				//ExecuteCmd(Path.Combine(Directory.GetCurrentDirectory(), "PsExec.exe"), $@"\\{_ipAddress} C:\Windows\System32\cmd.exe /K query user");
			});

			OpenRemoteExecution = ReactiveCommand.Create(() =>
			{
				//EnsureFileIsAvailable("PsExec.exe");
				//ExecuteCmd(Path.Combine(Directory.GetCurrentDirectory(), "PsExec.exe"), $@"\\{_ipAddress} C:\Windows\System32\cmd.exe");
			});

			OpenCDrive = ReactiveCommand.Create(() => { Process.Start($@"\\{_ipAddress}\C$"); });

			RebootComputer = ReactiveCommand.CreateFromObservable(() => _messages.Handle(new MessageInfo(MessageType.Question, $"Reboot {_ipAddress}?", "", "Yes", "No"))
                .Where(result => result == 0)
                .Do(_ => ExecuteFile(Path.Combine(ExecutionService.System32Path, "shutdown.exe"), $@"-r -f -m \\{_ipAddress} -t 0"))
                .ToSignal()
			);

			//StartRemoteControl = ReactiveCommand.Create(() => ExecuteFile(SettingsService.Current.RemoteControlClassicPath, $"1 {_ipAddress}"));

			//StartRemoteControl2012 = ReactiveCommand.Create(() => ExecuteFile(SettingsService.Current.RemoteControl2012Path, _ipAddress));

			KillRemoteControl = ReactiveCommand.Create(() => ExecuteFile(Path.Combine(ExecutionService.System32Path, "taskkill.exe"), $"/s {_ipAddress} /im rcagent.exe /f"));

			StartRemoteAssistance = ReactiveCommand.Create(() => ExecuteFile(Path.Combine(ExecutionService.System32Path, "msra.exe"), $"/offerra {_ipAddress}"));

			StartRdp = ReactiveCommand.Create(() => ExecuteFile(Path.Combine(ExecutionService.System32Path, "mstsc.exe"), $"/v {_ipAddress}"));

			_hostName = this.WhenAnyValue(vm => vm.IPAddress)
				.Where(ipAddress => ipAddress.HasValue())
				.Select(ipAddress => Dns.GetHostEntry(ipAddress).HostName)
				.CatchAndReturn("")
				.ToProperty(this, vm => vm.HostName, null);

            (this).WhenActivated((Action<CompositeDisposable>)(disposables =>
            {
                OpenCDrive
                    .ThrownExceptions
                    .SelectMany(ex => _messages.Handle(new MessageInfo(MessageType.Error, ex.Message, "Could not open location")))
                    .Subscribe()
                    .DisposeWith(disposables);

                Observable.Merge(
                        OpenLoggedOn.ThrownExceptions,
                        OpenLoggedOnPlus.ThrownExceptions,
                        OpenRemoteExecution.ThrownExceptions,
                        RebootComputer.ThrownExceptions,
                        StartRemoteControl.ThrownExceptions,
                        StartRemoteControl2012.ThrownExceptions,
                        KillRemoteControl.ThrownExceptions,
                        StartRemoteAssistance.ThrownExceptions,
                        StartRdp.ThrownExceptions)
                    .SelectMany(ex => _messages.Handle(new MessageInfo(MessageType.Error, ex.Message, "Could not launch external program")))
                    .Subscribe()
                    .DisposeWith(disposables);
            }));
		}



		public ReactiveCommand<Unit, Unit> OpenLoggedOn { get; private set; }
		public ReactiveCommand<Unit, Unit> OpenLoggedOnPlus { get; private set; }
        public ReactiveCommand<Unit, Unit> OpenRemoteExecution { get; private set; }
        public ReactiveCommand<Unit, Unit> OpenCDrive { get; private set; }
        public ReactiveCommand<Unit, Unit> RebootComputer { get; private set; }
        public ReactiveCommand<Unit, Unit> StartRemoteControl { get; private set; }
        public ReactiveCommand<Unit, Unit> StartRemoteControl2012 { get; private set; }
        public ReactiveCommand<Unit, Unit> KillRemoteControl { get; private set; }
        public ReactiveCommand<Unit, Unit> StartRemoteAssistance { get; private set; }
        public ReactiveCommand<Unit, Unit> StartRdp { get; private set; }
        public string HostName => _hostName.Value;
        public string IPAddress { get => _ipAddress; set => this.RaiseAndSetIfChanged(ref _ipAddress, value); }



        private void ExecuteCmd(string fileName, string arguments = "") => ExecuteFile(Path.Combine(ExecutionService.System32Path, "cmd.exe"), $@"/K {fileName} {arguments}");

		private void ExecuteFile(string fileName, string arguments = "")
		{
			if (File.Exists(fileName)) Process.Start(fileName, arguments);
			else throw new ArgumentException($"Could not find {fileName}");
		}



        private readonly ObservableAsPropertyHelper<string> _hostName;
        private string _ipAddress;
    }
}
