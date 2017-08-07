using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Reactive.Concurrency;
using MagnumOpus.ActiveDirectory;
using MagnumOpus.Dialog;
using MagnumOpus.User;

namespace MagnumOpus.Computer
{
    public class PermittedWorkstationsDialogViewModel : ViewModelBase, IDialog
    {
        public PermittedWorkstationsDialogViewModel()
        {
            AddComputer = ReactiveCommand.CreateFromObservable(
                () => AddComputerImpl(ComputerName),
                this.WhenAnyValue(vm => vm.ComputerName, computerName => computerName.HasValue(6)));

            RemoveComputer = ReactiveCommand.Create(
                () => _computers.Remove(SelectedComputer as string),
                this.WhenAnyValue(vm => vm.SelectedComputer).IsNotNull());

            RemoveAllComputers = ReactiveCommand.Create(
                () => _computers.Clear(),
                this.WhenAnyObservable(vm => vm._computers.CountChanged).Select(count => count > 0));

            Save = ReactiveCommand.CreateFromObservable(() => SaveImpl(User, _computers));

            Cancel = ReactiveCommand.Create(() => _close());

            this.WhenActivated(disposables =>
            {
                AddComputer
                    .Do(_ => ComputerName = "")
                    .Subscribe(computerName => _computers.Add(computerName))
                    .DisposeWith(disposables);

                Save
                    .Subscribe(_ => _close())
                    .DisposeWith(disposables);

                this
                    .WhenAnyValue(vm => vm.User)
                    .WhereNotNull()
                    .Select(user => user.Principal.PermittedWorkstations)
                    .Subscribe((System.DirectoryServices.AccountManagement.PrincipalValueCollection<string> x) =>
                    {
                        using (_computers.SuppressChangeNotifications()) _computers.AddRange(x);
                    })
                    .DisposeWith(disposables);

                Observable.Merge<(string Title, string Message)>(
                        AddComputer.ThrownExceptions.Select(ex => (("Could not add computer", ex.Message))),
                        RemoveComputer.ThrownExceptions.Select(ex => (("Could not remove computer", ex.Message))),
                        RemoveAllComputers.ThrownExceptions.Select(ex => (("Could not remove all computers", ex.Message))),
                        Save.ThrownExceptions.Select(ex => (("Could not save changes", ex.Message))),
                        Cancel.ThrownExceptions.Select(ex => (("Could not close dialog", ex.Message))))
                    .SelectMany(dialogContent => _messages.Handle(new MessageInfo(MessageType.Error, dialogContent.Message, dialogContent.Title)))
                    .Subscribe()
                    .DisposeWith(disposables);
            });
        }



        public ReactiveCommand<Unit, string> AddComputer { get; private set; }
        public ReactiveCommand<Unit, bool> RemoveComputer { get; private set; }
        public ReactiveCommand<Unit, Unit> RemoveAllComputers { get; private set; }
        public ReactiveCommand<Unit, Unit> Save { get; private set; }
        public ReactiveCommand<Unit, Unit> Cancel { get; private set; }
        public IReactiveDerivedList<string> Computers => _computers.CreateDerivedCollection(computerName => computerName, orderer: (one, two) => one.CompareTo(two));
        public UserObject User { get => _user; set => this.RaiseAndSetIfChanged(ref _user, value); }
        public string ComputerName { get => _computerName; set => this.RaiseAndSetIfChanged(ref _computerName, value); }
        public string SelectedComputer { get => _selectedComputer; set => this.RaiseAndSetIfChanged(ref _selectedComputer, value); }



        private IObservable<string> AddComputerImpl(string computerName) => Observable.Start(() =>
        {
            if (ActiveDirectoryService.Current.GetComputer(computerName).Wait() == null) throw new Exception("Could not find computer");
            return computerName;
        }, TaskPoolScheduler.Default);

        private IObservable<Unit> SaveImpl(UserObject user, IEnumerable<string> computers) => Observable.Start(() =>
        {
            user.Principal.PermittedWorkstations.Clear();
            foreach (var computer in computers) user.Principal.PermittedWorkstations.Add(computer);
            user.Principal.Save();
        }, TaskPoolScheduler.Default);



        public async Task Opening(Action close, object parameter)
        {
            _close = close;

            if (parameter is string s)
            {
                User = await ActiveDirectoryService.Current.GetUser(parameter as string);
            }
        }



        private readonly ReactiveList<string> _computers = new ReactiveList<string>();
        private UserObject _user;
        private string _computerName;
        private string _selectedComputer;
        private Action _close;
    }
}
