using ReactiveUI;
using System;
using System.DirectoryServices;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using MagnumOpus.Dialog;

namespace MagnumOpus.Group
{
    public class GroupNotesPanelViewModel : ViewModelBase
	{
		public GroupNotesPanelViewModel()
		{
			EnableEditing = ReactiveCommand.Create<Unit, bool>(_ => { _notesBackup = _notes; return true; });

			Save = ReactiveCommand.CreateFromObservable<Unit, bool>(_ => SaveImpl(_group, _notes).Select(__ => false));

			Cancel = ReactiveCommand.Create<Unit, bool>(_ => { Notes = _notesBackup; return false; });

			_isEditingEnabled = Observable.Merge(
				EnableEditing,
				Save,
				Cancel)
				.ToProperty(this, vm => vm.IsEditingEnabled);

            this.WhenActivated(disposables =>
            {
                this.WhenAnyValue(vm => vm.Group)
                    .WhereNotNull()
                    .Select(group => group.Notes?.Replace("\r", ""))
                    .Subscribe(notes => Notes = notes)
                    .DisposeWith(disposables);

                Observable.Merge(
                        EnableEditing.ThrownExceptions,
                        Save.ThrownExceptions,
                        Cancel.ThrownExceptions)
                    .SelectMany(ex => _messages.Handle(new MessageInfo(MessageType.Error, ex.Message)))
                    .Subscribe()
                    .DisposeWith(disposables);
            });
		}



		public ReactiveCommand<Unit, bool> EnableEditing { get; }
		public ReactiveCommand<Unit, bool> Save { get; }
        public ReactiveCommand<Unit, bool> Cancel { get; }
        public bool IsEditingEnabled => _isEditingEnabled.Value;
        public GroupObject Group { get => _group; set => this.RaiseAndSetIfChanged(ref _group, value); }
        public string Notes { get => _notes; set => this.RaiseAndSetIfChanged(ref _notes, value); }



        private IObservable<Unit> SaveImpl(GroupObject group, string newNotes) => Observable.Start(() =>
		{
			var directoryEntry = group.Principal.GetUnderlyingObject() as DirectoryEntry;
			directoryEntry.Properties["info"].Value = newNotes.Replace("\n", "\r\n");
			directoryEntry.CommitChanges();
		}, TaskPoolScheduler.Default);



		private readonly ObservableAsPropertyHelper<bool> _isEditingEnabled;
		private GroupObject _group;
		private string _notes;
		private string _notesBackup;
	}
}
