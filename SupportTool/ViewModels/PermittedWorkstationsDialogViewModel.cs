using ReactiveUI;
using SupportTool.Models;
using SupportTool.Services.ActiveDirectoryServices;
using SupportTool.Services.NavigationServices;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Data;

namespace SupportTool.ViewModels
{
	public class PermittedWorkstationsDialogViewModel : ViewModelBase, IDialog
    {
        private readonly ReactiveCommand<Unit, string> _addComputer;
        private readonly ReactiveCommand<Unit, bool> _removeComputer;
        private readonly ReactiveCommand<Unit, Unit> _removeAllComputers;
        private readonly ReactiveCommand<Unit, Unit> _save;
        private readonly ReactiveList<string> _computers;
        private readonly ListCollectionView _computersView;
        private UserObject _user;
        private string _computerName;
        private object _selectedComputer;
		private Action _close;



        public PermittedWorkstationsDialogViewModel()
        {
            _computers = new ReactiveList<string>();

            _computersView = new ListCollectionView(_computers);
            _computersView.SortDescriptions.Add(new SortDescription());

            _addComputer = ReactiveCommand.CreateFromObservable(
                () => AddComputerImpl(ComputerName),
                this.WhenAnyValue(x => x.ComputerName, x => x.HasValue(6)));
            _addComputer
                .Do(_ => ComputerName = "")
                .Subscribe(x => _computers.Add(x));
            _addComputer
                .ThrownExceptions
                .Subscribe(async ex => await _errorMessages.Handle(new MessageInfo(ex.Message, "Could not add computer")));

            _removeComputer = ReactiveCommand.Create(
            () => _computers.Remove(SelectedComputer as string),
            this.WhenAnyValue(x => x.SelectedComputer).Select(x => x != null));
            _removeComputer
                .ThrownExceptions
                .Subscribe(async ex => await _errorMessages.Handle(new MessageInfo(ex.Message, "Could not remove computer")));

            _removeAllComputers = ReactiveCommand.Create(
                () => _computers.Clear(),
            this.WhenAnyObservable(x => x._computers.CountChanged).Select(x => x > 0));
            _removeAllComputers
                .ThrownExceptions
                .Subscribe(async ex => await _errorMessages.Handle(new MessageInfo(ex.Message, "Could not remove computers")));

            _save = ReactiveCommand.CreateFromObservable(() => SaveImpl(User, _computers));
            _save
                .Subscribe(_ => _close());
            _save
                .ThrownExceptions
                .Subscribe(async ex => await _errorMessages.Handle(new MessageInfo(ex.Message, "Could not save")));

            this
                .WhenAnyValue(x => x.User)
                .WhereNotNull()
                .Select(x => x.Principal.PermittedWorkstations)
                .Subscribe(x =>
                {
                    using (_computers.SuppressChangeNotifications()) _computers.AddRange(x);
                });
        }



        public ReactiveCommand AddComputer => _addComputer;

        public ReactiveCommand RemoveComputer => _removeComputer;

        public ReactiveCommand RemoveAllComputers => _removeAllComputers;

        public ReactiveCommand Save => _save;

        public ReactiveList<string> Computers => _computers;

        public ListCollectionView ComputersView => _computersView;

        public UserObject User
        {
            get { return _user; }
            set { this.RaiseAndSetIfChanged(ref _user, value); }
        }

        public string ComputerName
        {
            get { return _computerName; }
            set { this.RaiseAndSetIfChanged(ref _computerName, value); }
        }

        public object SelectedComputer
        {
            get { return _selectedComputer; }
            set { this.RaiseAndSetIfChanged(ref _selectedComputer, value); }
        }



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

			if (parameter is string)
			{
				User = await ActiveDirectoryService.Current.GetUser(parameter as string);
			}
		}
	}
}
