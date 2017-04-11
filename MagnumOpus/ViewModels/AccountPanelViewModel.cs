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

namespace MagnumOpus.ViewModels
{
    public class AccountPanelViewModel : ViewModelBase
    {
        public AccountPanelViewModel()
        {
            _setNewPassword = ReactiveCommand.CreateFromObservable(
                () => SetNewPasswordImpl(),
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
                    .SelectMany(newPass => _messages.Handle(new MessageInfo(MessageType.Success, $"New password is: {newPass}", "Password set")))
                    .Subscribe()
                    .DisposeWith(disposables);

                _setNewSimplePassword
                    .SelectMany(newPass => _messages.Handle(MessageInfo.PasswordSetMessageInfo(newPass)))
                    .Subscribe()
                    .DisposeWith(disposables);

                _setNewComplexPassword
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
                    _expirePassword.ThrownExceptions,
                    _unlockAccount.ThrownExceptions,
                    _runLockoutStatus.ThrownExceptions,
                    _openPermittedWorkstations.ThrownExceptions,
                    _toggleEnabled.ThrownExceptions)
                    .SelectMany(ex => _messages.Handle(new MessageInfo(MessageType.Error, ex.Message)))
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



        private IObservable<string> SetNewPasswordImpl() => Observable.StartAsync(async () =>
        {
            var password = new string(NewPassword.ToArray());

            await ActiveDirectoryService.Current.SetPassword(User.Principal.SamAccountName, password, false);

            NewPassword = "";

            return password;
        });

        private IObservable<string> SetNewSimplePasswordImpl() => Observable.StartAsync(async () =>
        {
            var password = $"{DateTimeOffset.Now.DayOfWeek.ToNorwegianString()}{DateTimeOffset.Now.Minute.ToString("00")}";

            await ActiveDirectoryService.Current.SetPassword(User.Principal.SamAccountName, password);

            return password;
        });

        private IObservable<string> SetNewComplexPasswordImpl() => Observable.StartAsync(async () =>
        {
            var possibleChars = "abcdefgijkmnopqrstwxyzABCDEFGHJKLMNPQRSTWXYZ23456789*$-+?_&=!%{}/";
            var randGen = new Random(DateTime.Now.Second);
            var password = "";

            for (int i = 0; i < 16; i++) password += possibleChars[randGen.Next(possibleChars.Length)];

            await ActiveDirectoryService.Current.SetPassword(User.Principal.SamAccountName, password);

            return password;
        });



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
