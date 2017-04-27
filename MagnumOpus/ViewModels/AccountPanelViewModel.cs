using ReactiveUI;
using MagnumOpus.Models;
using MagnumOpus.Services.ActiveDirectoryServices;
using MagnumOpus.Services.FileServices;
using MagnumOpus.Services.SettingsServices;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Concurrency;

namespace MagnumOpus.ViewModels
{
    public class AccountPanelViewModel : ViewModelBase
    {
        public AccountPanelViewModel()
        {
            _setNewPassword = ReactiveCommand.CreateFromObservable(
                () => SetNewPasswordImpl(_newPassword),
                this.WhenAnyValue(x => x.User, y => y.NewPassword, (x, y) => x != null && y.HasValue()));

            _setNewSimplePassword = ReactiveCommand.CreateFromObservable(() => SetNewSimplePasswordImpl());

            _setNewComplexPassword = ReactiveCommand.CreateFromObservable(() => SetNewComplexPasswordImpl());

            _expirePassword = ReactiveCommand.CreateFromObservable(() => ActiveDirectoryService.Current.ExpirePassword(User.Principal.SamAccountName));

            _unlockAccount = ReactiveCommand.CreateFromObservable(() => ActiveDirectoryService.Current.UnlockUser(User.Principal.SamAccountName));

            _runLockoutStatus = ReactiveCommand.Create(() => ExecutionService.RunFileFromCache("LockoutStatus.exe", $"-u:{ActiveDirectoryService.Current.CurrentDomain}\\{User.Principal.SamAccountName}"));

            _openPermittedWorkstations = ReactiveCommand.CreateFromObservable(() => _dialogRequests.Handle(new DialogInfo(new Controls.PermittedWorkstationsDialog(), _user.Principal.SamAccountName)));

            _toggleEnabled = ReactiveCommand.CreateFromObservable(() => ActiveDirectoryService.Current.SetEnabled(User.Principal.SamAccountName, !User.Principal.Enabled ?? true));

            _openSplunk = ReactiveCommand.Create(() =>
            {
                Process.Start(string.Format(SettingsService.Current.SplunkUrl, User.Principal.SamAccountName));
            });

            this.WhenActivated(disposables =>
            {
                _setNewPassword
                    .ObserveOnDispatcher()
                    .Do(_ => NewPassword = "")
                    .SelectMany(newPass => _messages.Handle(new MessageInfo(MessageType.Success, $"New password is: {newPass}", "Password set")))
                    .Subscribe()
                    .DisposeWith(disposables);

                _setNewSimplePassword
                    .ObserveOnDispatcher()
                    .SelectMany(newPass => _messages.Handle(MessageInfo.PasswordSetMessageInfo(newPass)))
                    .Subscribe()
                    .DisposeWith(disposables);

                _setNewComplexPassword
                    .ObserveOnDispatcher()
                    .SelectMany(newPass => _messages.Handle(MessageInfo.PasswordSetMessageInfo(newPass)))
                    .Subscribe()
                    .DisposeWith(disposables);

                _expirePassword
                    .SelectMany(_ => _messages.Handle(new MessageInfo(MessageType.Success, "User must change password at next login", "Password expired")))
                    .Subscribe()
                    .DisposeWith(disposables);

                _unlockAccount
                    .SelectMany(_ =>
                    {
                        MessageBus.Current.SendMessage(_user.CN, ApplicationActionRequest.Refresh);
                        return _messages.Handle(new MessageInfo(MessageType.Success, "Account unlocked"));
                    })
                    .Subscribe()
                    .DisposeWith(disposables);

                _toggleEnabled
                    .Subscribe(_ => MessageBus.Current.SendMessage(_user.CN, ApplicationActionRequest.Refresh))
                    .DisposeWith(disposables);


                Observable.Merge(
                    _setNewPassword.ThrownExceptions,
                    _setNewSimplePassword.ThrownExceptions,
                    _setNewComplexPassword.ThrownExceptions)
                    .SelectMany(ex => _messages.Handle(MessageInfo.PasswordSetErrorMessageInfo(ex.Message)))
                    .Subscribe()
                    .DisposeWith(disposables);


                Observable.Merge(
                    _expirePassword.ThrownExceptions.Select(ex => ("Could not expire password", ex.Message)),
                    _unlockAccount.ThrownExceptions.Select(ex => ("Could not unlock acount", ex.Message)),
                    _runLockoutStatus.ThrownExceptions.Select(ex => ("Could not open LockOutStatus", ex.Message)),
                    _openPermittedWorkstations.ThrownExceptions.Select(ex => ("Could not open Permitted Workstations", ex.Message)),
                    _toggleEnabled.ThrownExceptions.Select(ex => ("Could not toggle enabled status", ex.Message)))
                    .SelectMany(x => _messages.Handle(new MessageInfo(MessageType.Error, x.Item2, x.Item1)))
                    .Subscribe()
                    .DisposeWith(disposables);
            });
        }



        public ReactiveCommand SetNewPassword => _setNewPassword;

        public ReactiveCommand SetNewSimplePassword => _setNewSimplePassword;

        public ReactiveCommand SetNewComplexPassword => _setNewComplexPassword;

        public ReactiveCommand ExpirePassword => _expirePassword;

        public ReactiveCommand UnlockAccount => _unlockAccount;

        public ReactiveCommand RunLockoutStatus => _runLockoutStatus;

        public ReactiveCommand OpenPermittedWorkstations => _openPermittedWorkstations;

        public ReactiveCommand ToggleEnabled => _toggleEnabled;

        public ReactiveCommand OpenSplunk => _openSplunk;

        public UserObject User { get => _user; set => this.RaiseAndSetIfChanged(ref _user, value); }

        public bool IsShowingNewPasswordOptions { get => _isShowingNewPasswordOptions; set => this.RaiseAndSetIfChanged(ref _isShowingNewPasswordOptions, value); }

        public string NewPassword { get => _newPassword; set => this.RaiseAndSetIfChanged(ref _newPassword, value); }



        private IObservable<string> SetNewPasswordImpl(string newPassword) => Observable.Return(newPassword)
            .SelectMany(password => ActiveDirectoryService.Current.SetPassword(User.Principal.SamAccountName, password, false, TaskPoolScheduler.Default).Select(_ => password));

        private IObservable<string> SetNewSimplePasswordImpl() => Observable.Return($"{DateTimeOffset.Now.DayOfWeek.ToNorwegianString()}{DateTimeOffset.Now.Minute.ToString("00")}")
            .SelectMany(password => ActiveDirectoryService.Current.SetPassword(User.Principal.SamAccountName, password).Select(_ => password));

        private IObservable<string> SetNewComplexPasswordImpl() => Observable.Start(() =>
        {
            var possibleChars = "abcdefgijkmnopqrstwxyzABCDEFGHJKLMNPQRSTWXYZ23456789*$-+?_&=!%{}/";
            var randGen = new Random(DateTime.Now.Second);
            var password = "";
            for (int i = 0; i < 16; i++) password += possibleChars[randGen.Next(possibleChars.Length)];
            return password;
        }, TaskPoolScheduler.Default)
        .SelectMany(password => ActiveDirectoryService.Current.SetPassword(User.Principal.SamAccountName, password, scheduler: CurrentThreadScheduler.Instance).Select(_ => password));



        private readonly ReactiveCommand<Unit, string> _setNewPassword;
        private readonly ReactiveCommand<Unit, string> _setNewSimplePassword;
        private readonly ReactiveCommand<Unit, string> _setNewComplexPassword;
        private readonly ReactiveCommand<Unit, Unit> _expirePassword;
        private readonly ReactiveCommand<Unit, Unit> _unlockAccount;
        private readonly ReactiveCommand<Unit, Unit> _runLockoutStatus;
        private readonly ReactiveCommand<Unit, Unit> _openPermittedWorkstations;
        private readonly ReactiveCommand<Unit, Unit> _toggleEnabled;
        private readonly ReactiveCommand<Unit, Unit> _openSplunk;
        private UserObject _user;
        private bool _isShowingNewPasswordOptions;
        private string _newPassword;
    }
}
