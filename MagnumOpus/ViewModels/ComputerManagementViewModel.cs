﻿using ReactiveUI;
using MagnumOpus.Models;
using MagnumOpus.Services.SettingsServices;
using System;
using System.Diagnostics;
using System.IO;
using System.Management.Automation;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using static MagnumOpus.Services.FileServices.ExecutionService;

namespace MagnumOpus.ViewModels
{
    public class ComputerManagementViewModel : ViewModelBase
    {
        public ComputerManagementViewModel()
        {
            _rebootComputer = ReactiveCommand.CreateFromObservable(() => _messages.Handle(new MessageInfo(MessageType.Question, $"Reboot {_computer.CN}?", "", "Yes", "No"))
                .Where(x => x == 0)
                .SelectMany(x => Observable.Start(() =>
                {
                    using (var powerShell = PowerShell.Create())
                    {
                        powerShell
                            .AddCommand("Restart-Computer")
                                .AddParameter("ComputerName", _computer.CN)
                                .AddParameter("Force")
                            .Invoke();
                    }
                }))
            );

            _runPSExec = ReactiveCommand.Create(() => RunInCmdFromCache("PsExec.exe", $@"\\{_computer.CN} C:\Windows\System32\cmd.exe"));

            _openCDrive = ReactiveCommand.Create(() => { Process.Start($@"\\{_computer.CN}\C$"); });

            _openSccm = ReactiveCommand.Create(() => { RunFile(SettingsService.Current.SCCMPath, _computer.CN); });

            this.WhenActivated(disposables =>
            {
                Observable.Merge(
                _rebootComputer.ThrownExceptions.Select(ex => ("Could not reboot computer", ex.Message)),
                _runPSExec.ThrownExceptions.Select(ex => ("Could not run PSExec", ex.Message)),
                _openCDrive.ThrownExceptions.Select(ex => ("Could not open C$ drive", ex.Message)),
                _openSccm.ThrownExceptions.Select(ex => ("Could not open SCCM", ex.Message)))
                .SelectMany(x => _messages.Handle(new MessageInfo(MessageType.Error, x.Item2, x.Item1)))
                .Subscribe()
                .DisposeWith(disposables);
            });
        }



        public ReactiveCommand RebootComputer => _rebootComputer;

        public ReactiveCommand RunPSExec => _runPSExec;

        public ReactiveCommand OpenCDrive => _openCDrive;

        public ReactiveCommand OpenSccm => _openSccm;

        public ComputerObject Computer { get => _computer; set => this.RaiseAndSetIfChanged(ref _computer, value); }



        private ReactiveCommand<Unit, Unit> _rebootComputer;
        private ReactiveCommand<Unit, Unit> _runPSExec;
        private ReactiveCommand<Unit, Unit> _openCDrive;
        private ReactiveCommand<Unit, Unit> _openSccm;
        private ComputerObject _computer;
    }
}
