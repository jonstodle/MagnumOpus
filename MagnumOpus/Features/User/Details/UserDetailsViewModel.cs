using ReactiveUI;
using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using MagnumOpus.ActiveDirectory;
using MagnumOpus.Dialog;
using MagnumOpus.Navigation;
using Splat;

namespace MagnumOpus.User
{
	public class UserDetailsViewModel : ViewModelBase
	{
        public UserDetailsViewModel()
        {
            ToggleOrganizationDetails = ReactiveCommand.Create<Unit, bool>(_ => !_isShowingOrganizationDetails.Value);

            OpenManager = ReactiveCommand.CreateFromTask(() => NavigationService.ShowPrincipalWindow(_manager.Value.Principal));

            OpenDirectReport = ReactiveCommand.CreateFromTask(
                () => NavigationService.ShowPrincipalWindow(_selectedDirectReport.Principal),
                this.WhenAnyValue(vm => vm.SelectedDirectReport).IsNotNull());

            var newUser = this.WhenAnyValue(vm => vm.User)
                .WhereNotNull()
                .Publish()
                .RefCount();

			_isAccountLocked = newUser
                .Select(user => user.Principal.IsAccountLockedOut())
                .ToProperty(this, vm => vm.IsAccountLocked);

            _passwordAge = newUser
                .Where(user => user.Principal.LastPasswordSet != null)
                .Select(user => DateTime.Now - user.Principal.LastPasswordSet.Value.ToLocalTime())
                .ToProperty(this, vm => vm.PasswordAge);

            _passwordStatus = Observable.CombineLatest(
                    newUser,
                    this.WhenAnyValue(vm => vm.PasswordAge),
                    (user, passwordAge) =>
                    {
                        if (user.Principal.PasswordNeverExpires) return "Password never expires";
                        else if ((user.Principal.LastPasswordSet ?? DateTime.MinValue) == DateTime.MinValue) return "Password must change";
                        else return $"Password age: {passwordAge.Days}d {passwordAge.Hours}h {passwordAge.Minutes}m";
                    })
                .ToProperty(this, vm => vm.PasswordStatus);

            _passwordMaxAge = newUser
                .SelectMany(user => user.Principal.PasswordNeverExpires || (user.Principal.LastPasswordSet ?? DateTime.MinValue) == DateTime.MinValue
                    ? Observable.Return(TimeSpan.Zero)
                    : Locator.Current.GetService<ADFacade>().GetMaxPasswordAge(user.Principal.SamAccountName, TaskPoolScheduler.Default))
                .ObserveOnDispatcher()
                .ToProperty(this, vm => vm.PasswordMaxAge);

            _isShowingOrganizationDetails = ToggleOrganizationDetails
                .ToProperty(this, x => x.IsShowingOrganizationDetails);

            _manager = newUser
                .SelectMany(user => user.GetManager())
                .ObserveOnDispatcher()
                .ToProperty(this, vm => vm.Manager);

            this.WhenActivated(disposables =>
            {
                Observable.Zip(
                        newUser,
                        this.WhenAnyValue(vm => vm.IsShowingOrganizationDetails).Where(true),
                        (user, _) => user.GetDirectReports())
                    .Do(_ => _directReports.Clear())
                    .Switch()
                    .ObserveOnDispatcher()
                    .Subscribe(user => _directReports.Add(user))
                    .DisposeWith(disposables);

                Observable.Merge<(string Title, string Message)>(
                        ToggleOrganizationDetails.ThrownExceptions.Select(ex => (("Could not toggle visibility", ex.Message))),
                        OpenManager.ThrownExceptions.Select(ex => (("Could not open manager", ex.Message))))
                    .SelectMany(dialogContent => _messages.Handle(new MessageInfo(MessageType.Error, dialogContent.Message, dialogContent.Title)))
                    .Subscribe()
                    .DisposeWith(disposables);
            });
        }



        public ReactiveCommand<Unit, bool> ToggleOrganizationDetails { get; }
        public ReactiveCommand<Unit, Unit> OpenManager { get; }
        public ReactiveCommand<Unit, Unit> OpenDirectReport { get; }
        public IReactiveDerivedList<UserObject> DirectReports => _directReports.CreateDerivedCollection(user => user, orderer: (one, two) => string.Compare(one.Principal.Name, two.Principal.Name, StringComparison.OrdinalIgnoreCase));
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
