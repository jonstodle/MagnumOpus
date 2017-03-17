using ReactiveUI;
using MagnumOpus.Models;
using MagnumOpus.Services.ActiveDirectoryServices;
using MagnumOpus.Services.NavigationServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace MagnumOpus.ViewModels
{
    public class PermittedWorkstationsDialogViewModel : ViewModelBase, IDialog
    {
        public PermittedWorkstationsDialogViewModel()
        {
            _addComputer = ReactiveCommand.CreateFromObservable(
                () => AddComputerImpl(ComputerName),
                this.WhenAnyValue(x => x.ComputerName, x => x.HasValue(6)));

            _removeComputer = ReactiveCommand.Create(
                () => _computers.Remove(SelectedComputer as string),
                this.WhenAnyValue(x => x.SelectedComputer).Select(x => x != null));

            _removeAllComputers = ReactiveCommand.Create(
                () => _computers.Clear(),
                this.WhenAnyObservable(x => x._computers.CountChanged).Select(x => x > 0));

            _save = ReactiveCommand.CreateFromObservable(() => SaveImpl(User, _computers));

            _cancel = ReactiveCommand.Create(() => _close());

            this.WhenActivated(disposables =>
            {
                _addComputer
                    .Do(_ => ComputerName = "")
                    .Subscribe(x => _computers.Add(x))
                    .DisposeWith(disposables);

                _save
                    .Subscribe(_ => _close())
                    .DisposeWith(disposables);

                this
                    .WhenAnyValue(x => x.User)
                    .WhereNotNull()
                    .Select(x => x.Principal.PermittedWorkstations)
                    .Subscribe(x =>
                    {
                        using (_computers.SuppressChangeNotifications()) _computers.AddRange(x);
                    })
                    .DisposeWith(disposables);

                Observable.Merge(
                _addComputer.ThrownExceptions,
                _removeComputer.ThrownExceptions,
                _removeAllComputers.ThrownExceptions,
                _save.ThrownExceptions,
                _cancel.ThrownExceptions)
                .Subscribe(async ex => await _errorMessages.Handle(new MessageInfo(ex.Message)))
                .DisposeWith(disposables);
            });
        }



        public ReactiveCommand AddComputer => _addComputer;

        public ReactiveCommand RemoveComputer => _removeComputer;

        public ReactiveCommand RemoveAllComputers => _removeAllComputers;

        public ReactiveCommand Save => _save;

        public ReactiveCommand Cancel => _cancel;

        public IReactiveDerivedList<string> Computers => _computers.CreateDerivedCollection(x => x, orderer: (one, two) => one.CompareTo(two));

        public UserObject User { get => _user; set => this.RaiseAndSetIfChanged(ref _user, value); }

        public string ComputerName { get => _computerName; set => this.RaiseAndSetIfChanged(ref _computerName, value); }

        public string SelectedComputer { get => _selectedComputer; set => this.RaiseAndSetIfChanged(ref _selectedComputer, value); }



        private IObservable<string> AddComputerImpl(string computerName) => Observable.Start(() =>
        {
            if (ActiveDirectoryService.Current.GetComputer(computerName).Wait() == null) throw new Exception("Could not find computer");
            return computerName;
        });

        private IObservable<Unit> SaveImpl(UserObject user, IEnumerable<string> computers) => Observable.Start(() =>
        {
            user.Principal.PermittedWorkstations.Clear();
            foreach (var computer in computers) user.Principal.PermittedWorkstations.Add(computer);
            user.Principal.Save();
        });



        public async Task Opening(Action close, object parameter)
        {
            _close = close;

            if (parameter is string s)
            {
                User = await ActiveDirectoryService.Current.GetUser(parameter as string);
            }
        }



        private readonly ReactiveCommand<Unit, string> _addComputer;
        private readonly ReactiveCommand<Unit, bool> _removeComputer;
        private readonly ReactiveCommand<Unit, Unit> _removeAllComputers;
        private readonly ReactiveCommand<Unit, Unit> _save;
        private readonly ReactiveCommand<Unit, Unit> _cancel;
        private readonly ReactiveList<string> _computers = new ReactiveList<string>();
        private UserObject _user;
        private string _computerName;
        private string _selectedComputer;
        private Action _close;
    }
}
