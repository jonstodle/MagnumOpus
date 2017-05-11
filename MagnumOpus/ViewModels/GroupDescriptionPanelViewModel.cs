using ReactiveUI;
using MagnumOpus.Models;
using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Concurrency;

namespace MagnumOpus.ViewModels
{
    public class GroupDescriptionPanelViewModel : ViewModelBase
    {
        public GroupDescriptionPanelViewModel()
        {
            Save = ReactiveCommand.CreateFromObservable(() => SaveImpl(_group, _description));

            Cancel = ReactiveCommand.Create(() => { Description = _group?.Description; });

            _hasDescriptionChanged = Observable.CombineLatest(
                this.WhenAnyValue(x => x.Description),
                this.WhenAnyValue(y => y.Group).WhereNotNull(),
                (x, y) => x != y.Description)
                .ToProperty(this, x => x.HasDescriptionChanged);

            (this).WhenActivated((Action<CompositeDisposable>)(disposables =>
            {
                (this).WhenAnyValue(x => x.Group)
                .WhereNotNull()
                .Subscribe(x => Description = x.Description)
                .DisposeWith(disposables);

                Observable.Merge(
                    Observable.Select(Save.ThrownExceptions, ex => (("Could not save changes", ex.Message))),
                    Cancel.ThrownExceptions.Select(ex => (("Could not reverse changes", ex.Message))))
                    .SelectMany(((string, string) x) => _messages.Handle(new MessageInfo(MessageType.Error, x.Item2, x.Item1)))
                    .Subscribe()
                    .DisposeWith(disposables);
            }));
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
