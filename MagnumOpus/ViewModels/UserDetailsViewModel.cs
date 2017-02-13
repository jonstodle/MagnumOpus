using ReactiveUI;
using MagnumOpus.Models;
using MagnumOpus.Services.NavigationServices;
using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace MagnumOpus.ViewModels
{
	public class UserDetailsViewModel : ViewModelBase
	{
        public UserDetailsViewModel()
        {
            _toggleOrganizationDetails = ReactiveCommand.Create<Unit, bool>(_ => !_isShowingOrganizationDetails.Value);

            _openManager = ReactiveCommand.CreateFromTask(() => NavigationService.ShowPrincipalWindow(_manager.Value.Principal));

            _openDirectReport = ReactiveCommand.CreateFromTask(
                () => NavigationService.ShowPrincipalWindow(_selectedDirectReport.Principal),
                this.WhenAnyValue(x => x.SelectedDirectReport).Select(x => x != null));

            var newUser = this.WhenAnyValue(x => x.User)
                .WhereNotNull()
                .Publish()
                .RefCount();

			_isAccountLocked = newUser
                .Select(x => x.Principal.IsAccountLockedOut())
                .ToProperty(this, x => x.IsAccountLocked);

            _passwordAge = newUser
                .Where(x => x.Principal.LastPasswordSet != null)
                .Select(x => DateTime.Now - x.Principal.LastPasswordSet.Value.ToLocalTime())
                .ToProperty(this, x => x.PasswordAge);

            _isShowingOrganizationDetails = _toggleOrganizationDetails
                .ToProperty(this, x => x.IsShowingOrganizationDetails);

            _manager = newUser
                .SelectMany(x => x.GetManager())
                .ObserveOnDispatcher()
                .ToProperty(this, x => x.Manager);

            this.WhenActivated(disposables =>
            {
                Observable.Zip(
                newUser,
                this.WhenAnyValue(x => x.IsShowingOrganizationDetails).Where(x => x),
                (usr, _) => usr.GetDirectReports())
                .Do(_ => _directReports.Clear())
                .Switch()
                .ObserveOnDispatcher()
                .Subscribe(x => _directReports.Add(x))
                .DisposeWith(disposables);

                Observable.Merge(
                    _toggleOrganizationDetails.ThrownExceptions,
                    _openManager.ThrownExceptions)
                    .Subscribe(async ex => await _errorMessages.Handle(new MessageInfo(ex.Message)))
                    .DisposeWith(disposables);
            });
        }



        public ReactiveCommand ToggleOrganizationDetails => _toggleOrganizationDetails;

        public ReactiveCommand OpenManager => _openManager;

        public ReactiveCommand OpenDirectReport => _openDirectReport;

        public IReactiveDerivedList<UserObject> DirectReports => _directReports.CreateDerivedCollection(x => x, orderer: (one, two) => one.Principal.Name.CompareTo(two.Principal.Name));

        public bool IsAccountLocked => _isAccountLocked.Value;

        public TimeSpan PasswordAge => _passwordAge.Value;

        public bool IsShowingOrganizationDetails => _isShowingOrganizationDetails.Value;

        public UserObject Manager => _manager.Value;

        public UserObject User { get => _user; set => this.RaiseAndSetIfChanged(ref _user, value); }

        public UserObject SelectedDirectReport { get => _selectedDirectReport; set => this.RaiseAndSetIfChanged(ref _selectedDirectReport, value); }



        private readonly ReactiveCommand<Unit, bool> _toggleOrganizationDetails;
        private readonly ReactiveCommand<Unit, Unit> _openManager;
        private readonly ReactiveCommand<Unit, Unit> _openDirectReport;
        private readonly ReactiveList<UserObject> _directReports = new ReactiveList<UserObject>();
        private readonly ObservableAsPropertyHelper<bool> _isAccountLocked;
        private readonly ObservableAsPropertyHelper<TimeSpan> _passwordAge;
        private readonly ObservableAsPropertyHelper<bool> _isShowingOrganizationDetails;
        private readonly ObservableAsPropertyHelper<UserObject> _manager;
        private UserObject _user;
        private UserObject _selectedDirectReport;
    }
}
