using Microsoft.Win32;
using ReactiveUI;
using SupportTool.Models;
using SupportTool.Services.ExportServices;
using SupportTool.Services.NavigationServices;
using System;
using System.ComponentModel;
using System.DirectoryServices;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Data;

namespace SupportTool.ViewModels
{
	public class ComputerGroupsViewModel : ViewModelBase
    {
		private readonly ReactiveCommand<Unit, Unit> _openEditMemberOf;
		private readonly ReactiveCommand<Unit, Unit> _saveDirectGroups;
		private readonly ReactiveCommand<Unit, Unit> _findDirectGroup;
        private readonly ReactiveList<string> _directGroups;
        private readonly ListCollectionView _directGroupsCollectionView;
        private ComputerObject _computer;
        private bool _isShowingDirectGroups;
		private object _selectedDirectGroup;



        public ComputerGroupsViewModel()
        {
            _directGroups = new ReactiveList<string>();

            _directGroupsCollectionView = new ListCollectionView(_directGroups);
            _directGroupsCollectionView.SortDescriptions.Add(new SortDescription());

			_openEditMemberOf = ReactiveCommand.CreateFromTask(async () => await _dialogRequests.Handle(new Models.DialogInfo(new Controls.EditMemberOfDialog(), _computer.Principal.SamAccountName)));

			_saveDirectGroups = ReactiveCommand.CreateFromTask(async () =>
			{
				var saveFileDialog = new SaveFileDialog { Filter = ExcelService.ExcelFileFilter };
				if (saveFileDialog.ShowDialog()  == true)
				{
					await ExcelService.SaveGroupsToExcelFile(_directGroups, saveFileDialog.FileName);
				}
			});
			_saveDirectGroups
				.ThrownExceptions
				.Subscribe(async ex => await _errorMessages.Handle(new MessageInfo(ex.Message)));

			_findDirectGroup = ReactiveCommand.CreateFromTask(() => NavigationService.ShowWindow<Views.GroupWindow>(_selectedDirectGroup as string));

            Observable.Merge(
                this.WhenAnyValue(x => x.Computer).WhereNotNull(),
               _openEditMemberOf.Select(_ => Computer))
                .Do(_ => DirectGroups.Clear())
                .SelectMany(x => GetDirectGroups(x).SubscribeOn(RxApp.TaskpoolScheduler))
				.Select(x => x.Properties.Get<string>("cn"))
                .ObserveOnDispatcher()
                .Subscribe(x => DirectGroups.Add(x));
        }



		public ReactiveCommand OpenEditMemberOf => _openEditMemberOf;

		public ReactiveCommand SaveDirectGroups => _saveDirectGroups;

		public ReactiveCommand FindDirectGroup => _findDirectGroup;

        public ReactiveList<string> DirectGroups => _directGroups;

        public ListCollectionView DirectGroupsCollectionView => _directGroupsCollectionView;

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

		public object SelectedDirectGroup
		{
			get { return _selectedDirectGroup; }
			set { this.RaiseAndSetIfChanged(ref _selectedDirectGroup, value); }
		}



        private IObservable<DirectoryEntry> GetDirectGroups(ComputerObject computer) => computer.Principal.GetGroups()
			.ToObservable()
			.Select(x => x.GetUnderlyingObject() as DirectoryEntry);
    }
}
