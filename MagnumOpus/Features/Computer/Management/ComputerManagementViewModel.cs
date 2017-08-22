using ReactiveUI;
using System;
using System.Diagnostics;
using System.Management.Automation;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using MagnumOpus.Dialog;
using MagnumOpus.Settings;

using static MagnumOpus.FileHelpers.ExecutionService;

namespace MagnumOpus.Computer
{
    public class ComputerManagementViewModel : ViewModelBase
    {
        public ComputerManagementViewModel()
        {
            RebootComputer = ReactiveCommand.CreateFromObservable(() => _messages.Handle(new MessageInfo(MessageType.Question, $"Reboot {_computer.CN}?", "", "Yes", "No"))
                .Where(result => result == 0)
                .SelectMany(_ => Observable.Start(() =>
                {
                    using (var powerShell = PowerShell.Create())
                    {
                        powerShell
                            .AddCommand("Restart-Computer")
                                .AddParameter("ComputerName", _computer.CN)
                                .AddParameter("Force")
                            .Invoke();
                    }
                }, TaskPoolScheduler.Default))
            );

            RunPSExec = ReactiveCommand.Create(() => RunInCmdFromCache("PsExec", "PsExec.exe", $@"\\{_computer.CN} C:\Windows\System32\cmd.exe"));

            OpenCDrive = ReactiveCommand.Create(() => { Process.Start($@"\\{_computer.CN}\C$"); });

            OpenSccm = ReactiveCommand.Create(() => { RunFile(SettingsService.Current.SCCMPath, _computer.CN); });

            this.WhenActivated(disposables =>
            {
                Observable.Merge<(string Title, string Message)>(
                    RebootComputer.ThrownExceptions.Select(ex => ("Could not reboot computer", ex.Message)),
                    RunPSExec.ThrownExceptions.Select(ex => ("Could not run PSExec", ex.Message)),
                    OpenCDrive.ThrownExceptions.Select(ex => ("Could not open C$ drive", ex.Message)),
                    OpenSccm.ThrownExceptions.Select(ex => ("Could not open SCCM", ex.Message)))
                .SelectMany(dialogContent => _messages.Handle(new MessageInfo(MessageType.Error, dialogContent.Message, dialogContent.Title)))
                .Subscribe()
                .DisposeWith(disposables);
            });
        }



        public ReactiveCommand<Unit, Unit> RebootComputer { get; }
        public ReactiveCommand<Unit, Unit> RunPSExec { get; }
        public ReactiveCommand<Unit, Unit> OpenCDrive { get; }
        public ReactiveCommand<Unit, Unit> OpenSccm { get; }
        public ComputerObject Computer { get => _computer; set => this.RaiseAndSetIfChanged(ref _computer, value); }

        

        private ComputerObject _computer;
    }
}
