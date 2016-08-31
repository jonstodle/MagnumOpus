using ReactiveUI;
using SupportTool.Helpers;
using SupportTool.Models;
using SupportTool.Services.ActiveDirectoryServices;
using SupportTool.Services.DialogServices;
using SupportTool.Services.NavigationServices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SupportTool.ViewModels
{
    public class AccountPanelViewModel : ReactiveObject
    {
        private readonly ReactiveCommand<Unit, string> setNewPassword;
        private readonly ReactiveCommand<Unit, string> setNewSimplePassword;
        private readonly ReactiveCommand<Unit, string> setNewComplexPassword;
        private readonly ReactiveCommand<Unit, Unit> expirePassword;
        private readonly ReactiveCommand<Unit, Unit> unlockAccount;
        private readonly ReactiveCommand<Unit, Unit> runLockoutStatus;
        private readonly ReactiveCommand<Unit, Unit> openSplunk;
        private readonly ReactiveCommand<Unit, Unit> openPermittedWorkstations;
        private UserObject user;
        private bool isShowingNewPasswordOptions;
        private string newPassword;



        public AccountPanelViewModel()
        {
            setNewPassword = ReactiveCommand.CreateFromObservable(
                () => SetNewPasswordImpl(),
                this.WhenAnyValue(x => x.User, y => y.NewPassword, (x, y) => x != null && y.HasValue()));
            setNewPassword
                .Subscribe(newPass => DialogService.ShowInfo($"New password is: {newPass}", "Password set"));
            setNewPassword
                .ThrownExceptions
                .Subscribe(ex => DialogService.ShowError(ex.Message, "Couldn't set password"));

            setNewSimplePassword = ReactiveCommand.CreateFromObservable(() => SetNewSimplePasswordImpl());
            setNewSimplePassword
                .Subscribe(newPass => DialogService.ShowPasswordSetMessage(newPass));
            setNewSimplePassword
                .ThrownExceptions
                .Subscribe(_ => DialogService.ShowPasswordSetErrorMessage());

            setNewComplexPassword = ReactiveCommand.CreateFromObservable(() => SetNewComplexPasswordImpl());
            setNewComplexPassword
                .Subscribe(newPass => DialogService.ShowPasswordSetMessage(newPass));
            setNewComplexPassword
                .ThrownExceptions
                .Subscribe(_ => DialogService.ShowPasswordSetErrorMessage());

            expirePassword = ReactiveCommand.CreateFromObservable(() => ActiveDirectoryService.Current.ExpirePassword(User.Principal.SamAccountName));
            expirePassword
                .Subscribe(_ => DialogService.ShowInfo("User must change password at next login", "Password expired"));
            expirePassword
                .ThrownExceptions
                .Subscribe(ex => DialogService.ShowError(ex.Message));

            unlockAccount = ReactiveCommand.CreateFromObservable(() => ActiveDirectoryService.Current.UnlockUser(User.Principal.SamAccountName));
            unlockAccount
                .Subscribe(_ => 
                {
                    MessageBus.Current.SendMessage(ApplicationActionRequest.Refresh);
                    DialogService.ShowInfo("Account unlocked");
                });
            unlockAccount
                .ThrownExceptions
                .Subscribe(ex => DialogService.ShowError(ex.Message));

            runLockoutStatus = ReactiveCommand.Create(() =>
            {
                if (!File.Exists("LockoutStatus.exe"))
                {
                    Application.GetResourceStream(new Uri("pack://application:,,,/Executables/LockoutStatus.exe")).WriteToDisk("LockoutStatus.exe");
                }
                Process.Start("LockoutStatus.exe", $"-u:sikt\\{User.Principal.SamAccountName}");
            });

            openSplunk = ReactiveCommand.Create(() =>
            {
                Process.Start($"https://sd3-splunksh-03.sikt.sykehuspartner.no/en-us/app/splunk_app_windows_infrastructure/search?q=search%20eventtype%3Dmsad-account-lockout%20user%3D\"{User.Principal.SamAccountName}\"%20dest_nt_domain%3D\"SIKT\"&earliest=-7d%40h&latest=now");
            });

            openPermittedWorkstations = ReactiveCommand.CreateFromTask(() => NavigationService.Current.NavigateTo<Views.PermittedWorkstationsWindow>(user.Principal.SamAccountName));

            this
                .WhenAnyValue(x => x.User)
                .Subscribe(_ => ResetValues());
        }



        public ReactiveCommand SetNewPassword => setNewPassword;

        public ReactiveCommand SetNewSimplePassword => setNewSimplePassword;

        public ReactiveCommand SetNewComplexPassword => setNewComplexPassword;

        public ReactiveCommand ExpirePassword => expirePassword;

        public ReactiveCommand UnlockAccount => unlockAccount;

        public ReactiveCommand RunLockoutStatus => runLockoutStatus;

        public ReactiveCommand OpenSplunk => openSplunk;

        public ReactiveCommand OpenPermittedWorkstations => openPermittedWorkstations;

        public UserObject User
        {
            get { return user; }
            set { this.RaiseAndSetIfChanged(ref user, value); }
        }

        public bool IsShowingNewPasswordOptions
        {
            get { return isShowingNewPasswordOptions; }
            set { this.RaiseAndSetIfChanged(ref isShowingNewPasswordOptions, value); }
        }

        public string NewPassword
        {
            get { return newPassword; }
            set { this.RaiseAndSetIfChanged(ref newPassword, value); }
        }



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

        private void ResetValues()
        {
            IsShowingNewPasswordOptions = false;
            NewPassword = "";
        }
    }
}
