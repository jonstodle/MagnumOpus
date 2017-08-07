using ReactiveUI;
using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using MagnumOpus.Dialog;

namespace MagnumOpus.Group
{
    public class GroupDescriptionPanelViewModel : ViewModelBase
    {
        public GroupDescriptionPanelViewModel()
        {
            Save = ReactiveCommand.CreateFromObservable(() => SaveImpl(_group, _description));

            Cancel = ReactiveCommand.Create(() => { Description = _group?.Description; });

            _hasDescriptionChanged = Observable.CombineLatest(
                    this.WhenAnyValue(vm => vm.Description),
                    this.WhenAnyValue(vm => vm.Group).WhereNotNull(),
                    (description, group) => description != group.Description)
                .ToProperty(this, vm => vm.HasDescriptionChanged);

            this.WhenActivated(disposables =>
            {
                this.WhenAnyValue(vm => vm.Group)
                    .WhereNotNull()
                    .Subscribe(group => Description = group.Description)
                    .DisposeWith(disposables);

                Observable.Merge<(string Title, string Message)>(
                        Save.ThrownExceptions.Select(ex => (("Could not save changes", ex.Message))),
                        Cancel.ThrownExceptions.Select(ex => (("Could not reverse changes", ex.Message))))
                    .SelectMany(dialogContent => _messages.Handle(new MessageInfo(MessageType.Error, dialogContent.Message, dialogContent.Title)))
                    .Subscribe()
                    .DisposeWith(disposables);
            });
        }



        public ReactiveCommand<Unit, Unit> Save { get; private set; }
        public ReactiveCommand<Unit, Unit> Cancel { get; private set; }
        public bool HasDescriptionChanged => _hasDescriptionChanged.Value;
        public GroupObject Group { get => _group; set => this.RaiseAndSetIfChanged(ref _group, value); }
        public string Description { get => _description; set => this.RaiseAndSetIfChanged(ref _description, value); }



        private IObservable<Unit> SaveImpl(GroupObject group, string description) => Observable.Start(() =>
        {
            group.Description = description;
            group.Principal.Save();
        }, TaskPoolScheduler.Default);



        private readonly ObservableAsPropertyHelper<bool> _hasDescriptionChanged;
        private GroupObject _group;
        private string _description;
    }
}
