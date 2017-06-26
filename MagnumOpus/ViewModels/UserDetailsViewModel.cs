using ReactiveUI;
using MagnumOpus.Models;
using MagnumOpus.Services.NavigationServices;
using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using MagnumOpus.Services.ActiveDirectoryServices;
using System.Reactive.Concurrency;

namespace MagnumOpus.ViewModels
{
	public class UserDetailsViewModel : ViewModelBase
	{
        public UserDetailsViewModel()
        {
            ToggleOrganizationDetails = ReactiveCommand.Create<Unit, bool>(_ => !_isShowingOrganizationDetails.Value);

            OpenManager = ReactiveCommand.CreateFromTask(() => NavigationService.ShowPrincipalWindow(_manager.Value.Principal));

            OpenDirectReport = ReactiveCommand.CreateFromTask(
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

            _passwordStatus = Observable.CombineLatest(
                newUser,
                this.WhenAnyValue(x => x.PasswordAge),
                (nu, pa) =>
                {
                    if (nu.Principal.PasswordNeverExpires) return "Password never expires";
                    else if ((nu.Principal.LastPasswordSet ?? DateTime.MinValue) == DateTime.MinValue) return "Password must change";
                    else return $"Password age: {pa.Days}d {pa.Hours}h {pa.Minutes}m";
                })
                .ToProperty(this, x => x.PasswordStatus);

            _passwordMaxAge = newUser
                .SelectMany(user => user.Principal.PasswordNeverExpires || (user.Principal.LastPasswordSet ?? DateTime.MinValue) == DateTime.MinValue
                    ? Observable.Return(TimeSpan.Zero)
                    : ActiveDirectoryService.Current.GetMaxPasswordAge(user.Principal.SamAccountName, TaskPoolScheduler.Default))
                .ObserveOnDispatcher()
                .ToProperty(this, vm => vm.PasswordMaxAge);

            _isShowingOrganizationDetails = ToggleOrganizationDetails
                .ToProperty(this, x => x.IsShowingOrganizationDetails);

            _manager = newUser
                .SelectMany(x => x.GetManager())
                .ObserveOnDispatcher()
                .ToProperty(this, x => x.Manager);

            (this).WhenActivated((Action<CompositeDisposable>)(disposables =>
            {
                Observable.Zip(
                newUser,
                (this).WhenAnyValue(x => x.IsShowingOrganizationDetails).Where(x => x),
                (usr, _) => usr.GetDirectReports())
                .Do((IObservable<UserObject> _) => _directReports.Clear())
                .Switch()
                .ObserveOnDispatcher()
                .Subscribe(x => _directReports.Add(x))
                .DisposeWith(disposables);

                Observable.Merge(
                    Observable.Select<Exception, (string, string)>(this.ToggleOrganizationDetails.ThrownExceptions, (Func<Exception, (string, string)>)(ex => ((string, string))(((string)"Could not toggle visibility", (string)ex.Message)))),
                    OpenManager.ThrownExceptions.Select(ex => (("Could not open manager", ex.Message))))
                    .SelectMany(((string, string) x) => _messages.Handle(new MessageInfo(MessageType.Error, x.Item2, x.Item1)))
                    .Subscribe()
                    .DisposeWith(disposables);
            }));
        }



        public ReactiveCommand<Unit, bool> ToggleOrganizationDetails { get; private set; }
        public ReactiveCommand<Unit, Unit> OpenManager { get; private set; }
        public ReactiveCommand<Unit, Unit> OpenDirectReport { get; private set; }
        public IReactiveDerivedList<UserObject> DirectReports => _directReports.CreateDerivedCollection(x => x, orderer: (one, two) => one.Principal.Name.CompareTo(two.Principal.Name));
        public bool IsAccountLocked => _isAccountLocked.Value;
        public TimeSpan PasswordAge => _passwordAge.Value;
        public string PasswordStatus => _passwordStatus.Value;
        public TimeSpan PasswordMaxAge => _passwordMaxAge.Value;
        public bool IsShowingOrganizationDetails => _isShowingOrganizationDetails.Value;
        public UserObject Manager => _manager.Value;
        public UserObject User { get => _user; set => this.RaiseAndSetIfChanged(ref _user, value); }
        public UserObject SelectedDirectReport { get => _selectedDirectReport; set => this.RaiseAndSetIfChanged(ref _selectedDirectReport, value); }



        private readonly ReactiveList<UserObject> _directReports = new ReactiveList<UserObject>();
        private readonly ObservableAsPropertyHelper<bool> _isAccountLocked;
        private readonly ObservableAsPropertyHelper<TimeSpan> _passwordAge;
        private readonly ObservableAsPropertyHelper<string> _passwordStatus;
        private readonly ObservableAsPropertyHelper<TimeSpan> _passwordMaxAge;
        private readonly ObservableAsPropertyHelper<bool> _isShowingOrganizationDetails;
        private readonly ObservableAsPropertyHelper<UserObject> _manager;
        private UserObject _user;
        private UserObject _selectedDirectReport;
    }
}
