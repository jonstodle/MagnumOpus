using ReactiveUI;
using MagnumOpus.Models;
using System;
using System.DirectoryServices;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Concurrency;

namespace MagnumOpus.ViewModels
{
    public class GroupNotesPanelViewModel : ViewModelBase
	{
		public GroupNotesPanelViewModel()
		{
			_enableEditing = ReactiveCommand.Create<Unit, bool>(_ => { _notesBackup = _notes; return true; });

			_save = ReactiveCommand.CreateFromObservable<Unit, bool>(_ => SaveImpl(_group, _notes).Select(x => false));

			_cancel = ReactiveCommand.Create<Unit, bool>(_ => { Notes = _notesBackup; return false; });

			_isEditingEnabled = Observable.Merge(
				_enableEditing,
				_save,
				_cancel)
				.ToProperty(this, x => x.IsEditingEnabled);

            this.WhenActivated(disposables =>
            {
                this.WhenAnyValue(x => x.Group)
                .WhereNotNull()
                .Select(x => x.Notes?.Replace("\r", ""))
                .Subscribe(x => Notes = x)
                .DisposeWith(disposables);

                Observable.Merge(
                    _enableEditing.ThrownExceptions,
                    _save.ThrownExceptions,
                    _cancel.ThrownExceptions)
                    .SelectMany(ex => _messages.Handle(new MessageInfo(MessageType.Error, ex.Message)))
                    .Subscribe()
                    .DisposeWith(disposables);
            });
		}



		public ReactiveCommand EnableEditing => _enableEditing;

		public ReactiveCommand Save => _save;

		public ReactiveCommand Cancel => _cancel;

		public bool IsEditingEnabled => _isEditingEnabled.Value;

        public GroupObject Group { get => _group; set => this.RaiseAndSetIfChanged(ref _group, value); }

        public string Notes { get => _notes; set => this.RaiseAndSetIfChanged(ref _notes, value); }



        private IObservable<Unit> SaveImpl(GroupObject group, string newNotes) => Observable.Start(() =>
		{
			var directoryEntry = group.Principal.GetUnderlyingObject() as DirectoryEntry;
			directoryEntry.Properties["info"].Value = newNotes.Replace("\n", "\r\n");
			directoryEntry.CommitChanges();
		}, TaskPoolScheduler.Default);



		private readonly ReactiveCommand<Unit, bool> _enableEditing;
		private readonly ReactiveCommand<Unit, bool> _save;
		private readonly ReactiveCommand<Unit, bool> _cancel;
		private readonly ObservableAsPropertyHelper<bool> _isEditingEnabled;
		private GroupObject _group;
		private string _notes;
		private string _notesBackup;
	}
}
