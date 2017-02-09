using Microsoft.Win32;
using ReactiveUI;
using SupportTool.Models;
using SupportTool.Services.ExportServices;
using SupportTool.Services.NavigationServices;
using System;
using System.DirectoryServices;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace SupportTool.ViewModels
{
    public class ComputerGroupsViewModel : ViewModelBase
    {
        public ComputerGroupsViewModel()
        {
            _openEditMemberOf = ReactiveCommand.CreateFromTask(async () => await _dialogRequests.Handle(new Models.DialogInfo(new Controls.EditMemberOfDialog(), _computer.Principal.SamAccountName)));

            _saveDirectGroups = ReactiveCommand.CreateFromTask(async () =>
            {
                var saveFileDialog = new SaveFileDialog { Filter = ExcelService.ExcelFileFilter };
                if (saveFileDialog.ShowDialog() == true)
                {
                    await ExcelService.SaveGroupsToExcelFile(_directGroups, saveFileDialog.FileName);
                }
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
                    _openEditMemberOf.ThrownExceptions,
                    _saveDirectGroups.ThrownExceptions,
                    _findDirectGroup.ThrownExceptions)
                    .Subscribe(async ex => await _errorMessages.Handle(new MessageInfo(ex.Message)))
                    .DisposeWith(disposables);
            });
        }



        public ReactiveCommand OpenEditMemberOf => _openEditMemberOf;

        public ReactiveCommand SaveDirectGroups => _saveDirectGroups;

        public ReactiveCommand FindDirectGroup => _findDirectGroup;

        public IReactiveDerivedList<string> DirectGroups => _directGroups.CreateDerivedCollection(x => x, orderer: (one, two) => one.CompareTo(two));

        public ComputerObject Computer
        {
            get { return _computer; }
            set { this.RaiseAndSetIfChanged(ref _computer, value); }
        }

        public bool IsShowingDirectGroups
        {
            get { return _isShowingDirectGroups; }
            set { this.RaiseAndSetIfChanged(ref _isShowingDirectGroups, value); }
        }

        public string SelectedDirectGroup
        {
            get { return _selectedDirectGroup; }
            set { this.RaiseAndSetIfChanged(ref _selectedDirectGroup, value); }
        }



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
