using Microsoft.Win32;
using ReactiveUI;
using MagnumOpus.Models;
using MagnumOpus.Services.ExportServices;
using MagnumOpus.Services.NavigationServices;
using System;
using System.DirectoryServices;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace MagnumOpus.ViewModels
{
    public class ComputerGroupsViewModel : ViewModelBase
    {
        public ComputerGroupsViewModel()
        {
            _openEditMemberOf = ReactiveCommand.CreateFromObservable(() => _dialogRequests.Handle(new Models.DialogInfo(new Controls.EditMemberOfDialog(), _computer.Principal.SamAccountName)));

            _saveDirectGroups = ReactiveCommand.CreateFromObservable(() =>
            {
                var saveFileDialog = new SaveFileDialog { Filter = ExcelService.ExcelFileFilter };
                return saveFileDialog.ShowDialog() ?? false ? ExcelService.SaveGroupsToExcelFile(_directGroups, saveFileDialog.FileName) : Observable.Return(Unit.Default);
            });

            _findDirectGroup = ReactiveCommand.CreateFromTask(() => NavigationService.ShowWindow<Views.GroupWindow>(_selectedDirectGroup));

            this.WhenActivated(disposables =>
            {
                Observable.Merge(
                this.WhenAnyValue(x => x.Computer).WhereNotNull(),
               _openEditMemberOf.Select(_ => Computer))
                .Do(_ => _directGroups.Clear())
                .SelectMany(x => GetDirectGroups(x).SubscribeOn(RxApp.TaskpoolScheduler))
                .Select(x => x.Properties.Get<string>("cn"))
                .ObserveOnDispatcher()
                .Subscribe(x => _directGroups.Add(x))
                .DisposeWith(disposables);

                Observable.Merge(
                    _openEditMemberOf.ThrownExceptions.Select(ex => ("Could not open dialog", ex.Message)),
                    _saveDirectGroups.ThrownExceptions.Select(ex => ("Could not save groups", ex.Message)),
                    _findDirectGroup.ThrownExceptions.Select(ex => ("Could not open group", ex.Message)))
                    .SelectMany(x => _messages.Handle(new MessageInfo(MessageType.Error, x.Item2, x.Item1)))
                    .Subscribe()
                    .DisposeWith(disposables);
            });
        }



        public ReactiveCommand OpenEditMemberOf => _openEditMemberOf;

        public ReactiveCommand SaveDirectGroups => _saveDirectGroups;

        public ReactiveCommand FindDirectGroup => _findDirectGroup;

        public IReactiveDerivedList<string> DirectGroups => _directGroups.CreateDerivedCollection(x => x, orderer: (one, two) => one.CompareTo(two));

        public ComputerObject Computer { get => _computer; set => this.RaiseAndSetIfChanged(ref _computer, value); }

        public bool IsShowingDirectGroups { get => _isShowingDirectGroups; set => this.RaiseAndSetIfChanged(ref _isShowingDirectGroups, value); }

        public string SelectedDirectGroup { get => _selectedDirectGroup; set => this.RaiseAndSetIfChanged(ref _selectedDirectGroup, value); }



        private IObservable<DirectoryEntry> GetDirectGroups(ComputerObject computer) => computer.Principal.GetGroups()
            .ToObservable()
            .Select(x => x.GetUnderlyingObject() as DirectoryEntry);



        private readonly ReactiveCommand<Unit, Unit> _openEditMemberOf;
        private readonly ReactiveCommand<Unit, Unit> _saveDirectGroups;
        private readonly ReactiveCommand<Unit, Unit> _findDirectGroup;
        private readonly ReactiveList<string> _directGroups = new ReactiveList<string>();
        private ComputerObject _computer;
        private bool _isShowingDirectGroups;
        private string _selectedDirectGroup;
    }
}
