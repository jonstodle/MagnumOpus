using Microsoft.Win32;
using ReactiveUI;
using System;
using System.DirectoryServices;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using DocumentFormat.OpenXml.Spreadsheet;
using MagnumOpus.Dialog;
using MagnumOpus.EditMemberOf;
using MagnumOpus.Group;
using MagnumOpus.Navigation;

namespace MagnumOpus.Computer
{
    public class ComputerGroupsViewModel : ViewModelBase
    {
        public ComputerGroupsViewModel()
        {
            OpenEditMemberOf = ReactiveCommand.CreateFromObservable(() => _dialogRequests.Handle(new DialogInfo(new EditMemberOfDialog(), _computer.Principal.SamAccountName)));

            SaveDirectGroups = ReactiveCommand.CreateFromObservable(() =>
            {
                var saveFileDialog = new SaveFileDialog { Filter = ExcelService.ExcelFileFilter, FileName = _computer.CN };
                return saveFileDialog.ShowDialog() ?? false ? ExcelService.SaveGroupsToExcelFile(_directGroups, saveFileDialog.FileName) : Observable.Return(Unit.Default);
            });

            FindDirectGroup = ReactiveCommand.CreateFromTask(() => NavigationService.ShowWindow<GroupWindow>(_selectedDirectGroup));

            (this).WhenActivated((Action<CompositeDisposable>)(disposables =>
            {
                Observable.Merge(
                        this.WhenAnyValue(vm => vm.Computer).WhereNotNull(),
                        OpenEditMemberOf.Select(_ => Computer))
                    .Do(_ => _directGroups.Clear())
                    .SelectMany(computerObject => GetDirectGroups(computerObject, TaskPoolScheduler.Default))
                    .Select(directoryEntry => directoryEntry.Properties.Get<string>("cn"))
                    .ObserveOnDispatcher()
                    .Subscribe(cn => _directGroups.Add(cn))
                    .DisposeWith(disposables);

                Observable.Merge<(string Title, string Message)>(
                        Observable.Select(OpenEditMemberOf.ThrownExceptions, ex => (("Could not open dialog", ex.Message))),
                        SaveDirectGroups.ThrownExceptions.Select(ex => (("Could not save groups", ex.Message))),
                        FindDirectGroup.ThrownExceptions.Select(ex => (("Could not open group", ex.Message))))
                    .SelectMany(dialogContent => _messages.Handle(new MessageInfo(MessageType.Error, dialogContent.Message, dialogContent.Title)))
                    .Subscribe()
                    .DisposeWith(disposables);
            }));
        }



        public ReactiveCommand<Unit, Unit> OpenEditMemberOf { get; private set; }
        public ReactiveCommand<Unit, Unit> SaveDirectGroups { get; private set; }
        public ReactiveCommand<Unit, Unit> FindDirectGroup { get; private set; }
        public IReactiveDerivedList<string> DirectGroups => _directGroups.CreateDerivedCollection(groupName => groupName, orderer: (one, two) => one.CompareTo(two));
        public ComputerObject Computer { get => _computer; set => this.RaiseAndSetIfChanged(ref _computer, value); }
        public bool IsShowingDirectGroups { get => _isShowingDirectGroups; set => this.RaiseAndSetIfChanged(ref _isShowingDirectGroups, value); }
        public string SelectedDirectGroup { get => _selectedDirectGroup; set => this.RaiseAndSetIfChanged(ref _selectedDirectGroup, value); }



        private IObservable<DirectoryEntry> GetDirectGroups(ComputerObject computer, IScheduler scheduler = null) => computer.Principal.GetGroups()
            .ToObservable(scheduler ?? TaskPoolScheduler.Default)
            .Select(principal => principal.GetUnderlyingObject() as DirectoryEntry);



        private readonly ReactiveList<string> _directGroups = new ReactiveList<string>();
        private ComputerObject _computer;
        private bool _isShowingDirectGroups;
        private string _selectedDirectGroup;
    }
}
