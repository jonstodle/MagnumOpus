using ReactiveUI;
using MagnumOpus.Models;
using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace MagnumOpus.ViewModels
{
    public class GroupDescriptionPanelViewModel : ViewModelBase
    {
        public GroupDescriptionPanelViewModel()
        {
            _save = ReactiveCommand.CreateFromObservable(() => SaveImpl(_group, _description));

            _cancel = ReactiveCommand.Create(() => { Description = _group?.Description; });

            _hasDescriptionChanged = Observable.CombineLatest(
                this.WhenAnyValue(x => x.Description),
                this.WhenAnyValue(y => y.Group).WhereNotNull(),
                (x, y) => x != y.Description)
                .ToProperty(this, x => x.HasDescriptionChanged);

            this.WhenActivated(disposables =>
            {
                this.WhenAnyValue(x => x.Group)
                .WhereNotNull()
                .Subscribe(x => Description = x.Description)
                .DisposeWith(disposables);

                Observable.Merge(
                    _save.ThrownExceptions,
                    _cancel.ThrownExceptions)
                    .SelectMany(ex => _messages.Handle(new MessageInfo(MessageType.Error, ex.Message)))
                    .Subscribe()
                    .DisposeWith(disposables);
            });
        }



        public ReactiveCommand Save => _save;

        public ReactiveCommand Cancel => _cancel;

        public bool HasDescriptionChanged => _hasDescriptionChanged.Value;

        public GroupObject Group { get => _group; set => this.RaiseAndSetIfChanged(ref _group, value); }

        public string Description { get => _description; set => this.RaiseAndSetIfChanged(ref _description, value); }



        private IObservable<Unit> SaveImpl(GroupObject group, string description) => Observable.Start(() =>
        {
            group.Description = description;
            group.Principal.Save();
        });



        private readonly ReactiveCommand<Unit, Unit> _save;
        private readonly ReactiveCommand<Unit, Unit> _cancel;
        private readonly ObservableAsPropertyHelper<bool> _hasDescriptionChanged;
        private GroupObject _group;
        private string _description;
    }
}
