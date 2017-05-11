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
			EnableEditing = ReactiveCommand.Create<Unit, bool>(_ => { _notesBackup = _notes; return true; });

			Save = ReactiveCommand.CreateFromObservable<Unit, bool>(_ => SaveImpl(_group, _notes).Select(x => false));

			Cancel = ReactiveCommand.Create<Unit, bool>(_ => { Notes = _notesBackup; return false; });

			_isEditingEnabled = Observable.Merge(
				EnableEditing,
				Save,
				Cancel)
				.ToProperty(this, x => x.IsEditingEnabled);

            (this).WhenActivated((Action<CompositeDisposable>)(disposables =>
            {
                (this).WhenAnyValue(x => x.Group)
                .WhereNotNull()
                .Select(x => x.Notes?.Replace("\r", ""))
                .Subscribe(x => Notes = x)
                .DisposeWith(disposables);

                Observable.Merge(
(IObservable<Exception>)this.EnableEditing.ThrownExceptions,
                    Save.ThrownExceptions,
                    Cancel.ThrownExceptions)
                    .SelectMany(ex => _messages.Handle(new MessageInfo(MessageType.Error, ex.Message)))
                    .Subscribe()
                    .DisposeWith(disposables);
            }));
		}



		public ReactiveCommand<Unit, bool> EnableEditing { get; private set; }
		public ReactiveCommand<Unit, bool> Save { get; private set; }
        public ReactiveCommand<Unit, bool> Cancel { get; private set; }
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
