using ReactiveUI;
using SupportTool.Models;
using SupportTool.Services.ActiveDirectoryServices;
using SupportTool.Services.FileServices;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace SupportTool.ViewModels
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

            _runLockoutStatus = ReactiveCommand.Create(() => ExecutionService.ExecuteInternalFile("LockoutStatus.exe", $"-u:{ActiveDirectoryService.Current.CurrentDomain}\\{User.Principal.SamAccountName}"));

            _openPermittedWorkstations = ReactiveCommand.CreateFromTask(async () => await _dialogRequests.Handle(new DialogInfo(new Controls.PermittedWorkstationsDialog(), _user.Principal.SamAccountName)));

            _toggleEnabled = ReactiveCommand.CreateFromObservable(() => ActiveDirectoryService.Current.SetEnabled(User.Principal.SamAccountName, !User.Principal.Enabled ?? true));

            _openSplunk = ReactiveCommand.Create(() =>
            {
                Process.Start($"https://sd3-splunksh-03.sikt.sykehuspartner.no/en-us/app/splunk_app_windows_infrastructure/search?q=search%20eventtype%3Dmsad-account-lockout%20user%3D\"{User.Principal.SamAccountName}\"%20dest_nt_domain%3D\"SIKT\"&earliest=-7d%40h&latest=now");
            });

            _openFindUser = ReactiveCommand.Create(() => ExecutionService.ExecuteInternalFile("FindUserNet.exe"));

            this.WhenActivated(disposables =>
            {
                _setNewPassword
                    .Subscribe(async newPass => await _infoMessages.Handle(new MessageInfo($"New password is: {newPass}", "Password set")))
                    .DisposeWith(disposables);

                _setNewSimplePassword
                    .Subscribe(async newPass => await _infoMessages.Handle(MessageInfo.PasswordSetMessageInfo(newPass)))
                    .DisposeWith(disposables);

                _setNewComplexPassword
                    .Subscribe(async newPass => await _infoMessages.Handle(MessageInfo.PasswordSetMessageInfo(newPass)))
                    .DisposeWith(disposables);

                _expirePassword
                    .Subscribe(async _ => await _infoMessages.Handle(new MessageInfo("User must change password at next login", "Password expired")))
                    .DisposeWith(disposables);

                _unlockAccount
                    .Subscribe(async _ =>
                    {
                        MessageBus.Current.SendMessage(_user.CN, ApplicationActionRequest.Refresh.ToString());
                        await _infoMessages.Handle(new MessageInfo("Account unlocked"));
                    })
                    .DisposeWith(disposables);

                _toggleEnabled
                    .Subscribe(_ => MessageBus.Current.SendMessage(_user.CN, ApplicationActionRequest.Refresh.ToString()))
                    .DisposeWith(disposables);


                Observable.Merge(
                    _setNewPassword.ThrownExceptions,
                    _setNewSimplePassword.ThrownExceptions,
                    _setNewComplexPassword.ThrownExceptions)
                    .Subscribe(async ex => await _errorMessages.Handle(MessageInfo.PasswordSetErrorMessageInfo(ex.Message)))
                    .DisposeWith(disposables);


                Observable.Merge(
                    _expirePassword.ThrownExceptions,
                    _unlockAccount.ThrownExceptions,
                    _runLockoutStatus.ThrownExceptions,
                    _openPermittedWorkstations.ThrownExceptions,
                    _toggleEnabled.ThrownExceptions,
                    _openFindUser.ThrownExceptions)
                    .Subscribe(async ex => await _errorMessages.Handle(new MessageInfo(ex.Message)))
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

        public ReactiveCommand OpenFindUser => _openFindUser;

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
        private readonly ReactiveCommand<Unit, Unit> _openFindUser;
        private UserObject _user;
        private bool _isShowingNewPasswordOptions;
        private string _newPassword;
    }
}
