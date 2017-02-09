using ReactiveUI;
using SupportTool.Models;
using System;
using System.Diagnostics;
using System.IO;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using static SupportTool.Services.FileServices.ExecutionService;

namespace SupportTool.ViewModels
{
	public class ComputerManagementViewModel : ViewModelBase
	{
		public ComputerManagementViewModel()
		{
			_rebootComputer = ReactiveCommand.CreateFromTask(async () =>
			{
				if (await _promptMessages.Handle(new MessageInfo($"Reboot {_computer.CN}?", "", "Yes", "No")) == 0)
				{
					ExecuteFile(Path.Combine(System32Path, "shutdown.exe"), $@"-r -f -m \\{_computer.CN} -t 0", false);
				}
			});

			_runPSExec = ReactiveCommand.Create(() => ExecuteInternalCmd("PsExec.exe", $@"\\{_computer.CN} C:\Windows\System32\cmd.exe"));

			_openCDrive = ReactiveCommand.Create(() => { Process.Start($@"\\{_computer.CN}\C$"); });

			_openSccm = ReactiveCommand.Create(() => { ExecuteFile(@"C:\Program Files\SCCM Tools\SCCM Client Center\SMSCliCtrV2.exe", _computer.CN); });

            this.WhenActivated(disposables =>
            {
                Observable.Merge(
                _rebootComputer.ThrownExceptions,
                _runPSExec.ThrownExceptions,
                _openCDrive.ThrownExceptions,
                _openSccm.ThrownExceptions)
                .Subscribe(async ex => await _errorMessages.Handle(new MessageInfo(ex.Message)))
                .DisposeWith(disposables);
            });
		}



		public ReactiveCommand RebootComputer => _rebootComputer;

		public ReactiveCommand RunPSExec => _runPSExec;

		public ReactiveCommand OpenCDrive => _openCDrive;

		public ReactiveCommand OpenSccm => _openSccm;

		public ComputerObject Computer
		{
			get { return _computer; }
			set { this.RaiseAndSetIfChanged(ref _computer, value); }
		}



        private ReactiveCommand<Unit, Unit> _rebootComputer;
        private ReactiveCommand<Unit, Unit> _runPSExec;
        private ReactiveCommand<Unit, Unit> _openCDrive;
        private ReactiveCommand<Unit, Unit> _openSccm;
        private ComputerObject _computer;
    }
}
