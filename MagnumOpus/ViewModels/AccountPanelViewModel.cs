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
            SetNewPassword = ReactiveCommand.CreateFromObservable(
                () => SetNewPasswordImpl(_newPassword),
                this.WhenAnyValue(x => x.User, y => y.NewPassword, (x, y) => x != null && y.HasValue()));

            SetNewSimplePassword = ReactiveCommand.CreateFromObservable(() => SetNewSimplePasswordImpl());

            SetNewComplexPassword = ReactiveCommand.CreateFromObservable(() => SetNewComplexPasswordImpl());

            ExpirePassword = ReactiveCommand.CreateFromObservable(() => ActiveDirectoryService.Current.ExpirePassword(User.Principal.SamAccountName));

            UnlockAccount = ReactiveCommand.CreateFromObservable(() => ActiveDirectoryService.Current.UnlockUser(User.Principal.SamAccountName));

            RunLockoutStatus = ReactiveCommand.Create(() => ExecutionService.RunFileFromCache("LockoutStatus", $"-u:{ActiveDirectoryService.Current.CurrentDomain}\\{User.Principal.SamAccountName}"));

            OpenPermittedWorkstations = ReactiveCommand.CreateFromObservable(() => _dialogRequests.Handle(new DialogInfo(new Controls.PermittedWorkstationsDialog(), _user.Principal.SamAccountName)));

            ToggleEnabled = ReactiveCommand.CreateFromObservable(() => ActiveDirectoryService.Current.SetEnabled(User.Principal.SamAccountName, !User.Principal.Enabled ?? true));

            OpenSplunk = ReactiveCommand.Create(() =>
            {
                Process.Start(string.Format(SettingsService.Current.SplunkUrl, ActiveDirectoryService.Current.CurrentDomainShortName, User.Principal.SamAccountName));
            });

            this.WhenActivated(disposables =>
            {
                SetNewPassword
                    .ObserveOnDispatcher()
                    .Do(_ => NewPassword = "")
                    .SelectMany(newPass => _messages.Handle(new MessageInfo(MessageType.Success, $"New password is: {newPass}", "Password set")))
                    .Subscribe()
                    .DisposeWith(disposables);

                SetNewSimplePassword
                    .ObserveOnDispatcher()
                    .SelectMany(newPass => _messages.Handle(MessageInfo.PasswordSetMessageInfo(newPass)))
                    .Subscribe()
                    .DisposeWith(disposables);

                SetNewComplexPassword
                    .ObserveOnDispatcher()
                    .SelectMany(newPass => _messages.Handle(MessageInfo.PasswordSetMessageInfo(newPass)))
                    .Subscribe()
                    .DisposeWith(disposables);

                ExpirePassword
                    .SelectMany(_ => _messages.Handle(new MessageInfo(MessageType.Success, "User must change password at next login", "Password expired")))
                    .Subscribe()
                    .DisposeWith(disposables);

                UnlockAccount
                    .SelectMany(_ =>
                    {
                        MessageBus.Current.SendMessage(_user.CN, ApplicationActionRequest.Refresh);
                        return _messages.Handle(new MessageInfo(MessageType.Success, "Account unlocked"));
                    })
                    .Subscribe()
                    .DisposeWith(disposables);

                ToggleEnabled
                    .Subscribe(_ => MessageBus.Current.SendMessage(_user.CN, ApplicationActionRequest.Refresh))
                    .DisposeWith(disposables);


                Observable.Merge(
                    SetNewPassword.ThrownExceptions,
                    SetNewSimplePassword.ThrownExceptions,
                    SetNewComplexPassword.ThrownExceptions)
                    .SelectMany(ex => _messages.Handle(MessageInfo.PasswordSetErrorMessageInfo(ex.Message)))
                    .Subscribe()
                    .DisposeWith(disposables);


                Observable.Merge(
                    ExpirePassword.ThrownExceptions.Select(ex => ("Could not expire password", ex.Message)),
                    UnlockAccount.ThrownExceptions.Select(ex => ("Could not unlock acount", ex.Message)),
                    RunLockoutStatus.ThrownExceptions.Select(ex => ("Could not open LockOutStatus", ex.Message)),
                    OpenPermittedWorkstations.ThrownExceptions.Select(ex => ("Could not open Permitted Workstations", ex.Message)),
                    ToggleEnabled.ThrownExceptions.Select(ex => ("Could not toggle enabled status", ex.Message)))
                    .SelectMany(x => _messages.Handle(new MessageInfo(MessageType.Error, x.Item2, x.Item1)))
                    .Subscribe()
                    .DisposeWith(disposables);
            });
        }



        public ReactiveCommand<Unit, string> SetNewPassword { get; private set; }
        public ReactiveCommand<Unit, string> SetNewSimplePassword { get; private set; }
        public ReactiveCommand<Unit, string> SetNewComplexPassword { get; private set; }
        public ReactiveCommand<Unit, Unit> ExpirePassword { get; private set; }
        public ReactiveCommand<Unit, Unit> UnlockAccount { get; private set; }
        public ReactiveCommand<Unit, Unit> RunLockoutStatus { get; private set; }
        public ReactiveCommand<Unit, Unit> OpenPermittedWorkstations { get; private set; }
        public ReactiveCommand<Unit, Unit> ToggleEnabled { get; private set; }
        public ReactiveCommand<Unit, Unit> OpenSplunk { get; private set; }
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



        private UserObject _user;
        private bool _isShowingNewPasswordOptions;
        private string _newPassword;
    }
}
