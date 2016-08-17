﻿using ReactiveUI;
using SupportTool.Helpers;
using SupportTool.Models;
using SupportTool.Services.ActiveDirectoryServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SupportTool.ViewModels
{
    public class PasswordPanelViewModel : ReactiveObject
    {
        private readonly Subject<Message> messages;

        private readonly ReactiveCommand<Unit, string> setNewPassword;
        private readonly ReactiveCommand<Unit, string> setNewSimplePassword;
        private readonly ReactiveCommand<Unit, string> setNewComplexPassword;
        private readonly ReactiveCommand<Unit, Unit> expirePassword;
        private readonly ReactiveCommand<Unit, Unit> unlockAccount;
        private UserObject user;
        private bool isShowingNewPasswordOptions;
        private string newPassword;



        public PasswordPanelViewModel()
        {
            messages = new Subject<Message>();

            setNewPassword = ReactiveCommand.CreateFromObservable(
                () => SetNewPasswordImpl(),
                this.WhenAnyValue(x => x.User, y => y.NewPassword, (x, y) => x != null && y.HasValue()));
            setNewPassword
                .Subscribe(newPass => messages.OnNext(Message.PasswordSet(newPass)));
            setNewPassword
                .ThrownExceptions
                .Subscribe(_ => messages.OnNext(Message.PasswordSetError()));

            setNewSimplePassword = ReactiveCommand.CreateFromObservable(() => SetNewSimplePasswordImpl());
            setNewSimplePassword
                .Subscribe(newPass => messages.OnNext(Message.PasswordSet(newPass)));
            setNewSimplePassword
                .ThrownExceptions
                .Subscribe(_ => messages.OnNext(Message.PasswordSetError()));

            setNewComplexPassword = ReactiveCommand.CreateFromObservable(() => SetNewComplexPasswordImpl());
            setNewComplexPassword
                .Subscribe(newPass => messages.OnNext(Message.PasswordSet(newPass)));
            setNewComplexPassword
                .ThrownExceptions
                .Subscribe(_ => messages.OnNext(Message.PasswordSetError()));

            expirePassword = ReactiveCommand.CreateFromObservable(() => ActiveDirectoryService.Current.ExpirePassword(User.Principal.SamAccountName));
            expirePassword
                .Subscribe(_ => messages.OnNext(Message.Info("User must change password at next login", "Password expired")));
            expirePassword
                .ThrownExceptions
                .Subscribe(ex => messages.OnNext(Message.Error(ex.Message)));

            unlockAccount = ReactiveCommand.CreateFromObservable(() => ActiveDirectoryService.Current.UnlockUser(User.Principal.SamAccountName));
            unlockAccount
                .Subscribe(_ => 
                {
                    messages.OnNext(Message.Info("Account unlocked"));
                    MessageBus.Current.SendMessage(ApplicationActionRequest.RefreshUser);
                });
            unlockAccount
                .ThrownExceptions
                .Subscribe(ex => messages.OnNext(Message.Error(ex.Message)));
        }



        public IObservable<Message> Messages => messages;

        public ReactiveCommand SetNewPassword => setNewPassword;

        public ReactiveCommand SetNewSimplePassword => setNewSimplePassword;

        public ReactiveCommand SetNewComplexPassword => setNewComplexPassword;

        public ReactiveCommand ExpirePassword => expirePassword;

        public ReactiveCommand UnlockAccount => unlockAccount;

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



        private IObservable<string> SetNewPasswordImpl() => Observable.Start(() =>
        {
            var password = new string(NewPassword.ToArray());

            ActiveDirectoryService.Current.SetPassword(User.Principal.SamAccountName, password).Wait();

            NewPassword = "";

            return password;
        });

        private IObservable<string> SetNewSimplePasswordImpl() => Observable.Start(() =>
        {
            var password = $"{DateTimeOffset.Now.DayOfWeek.ToNorwegianString()}{DateTimeOffset.Now.Minute.ToString("00")}";

            ActiveDirectoryService.Current.SetPassword(User.Principal.SamAccountName, password).Wait();

            return password;
        });

        private IObservable<string> SetNewComplexPasswordImpl() => Observable.Start(() =>
        {
            var possibleChars = "abcdefgijkmnopqrstwxyzABCDEFGHJKLMNPQRSTWXYZ23456789*$-+?_&=!%{}/";
            var randGen = new Random(DateTime.Now.Second);
            var password = "";

            for (int i = 0; i < 16; i++) password += possibleChars[randGen.Next(possibleChars.Length)];

            ActiveDirectoryService.Current.SetPassword(User.Principal.SamAccountName, password).Wait();

            return password;
        });
    }
}
