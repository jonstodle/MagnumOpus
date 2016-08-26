using ReactiveUI;
using SupportTool.Helpers;
using SupportTool.Models;
using SupportTool.Services.ActiveDirectoryServices;
using SupportTool.Services.DialogServices;
using SupportTool.Services.NavigationServices;
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
    public partial class MainWindowViewModel : ReactiveObject, INavigable
    {
        // User
        private readonly ReactiveCommand<Unit, string> userPaste;
        private readonly ReactiveCommand<Unit, UserObject> findUser;
        private readonly ReactiveCommand<Unit, Unit> userPasteAndFind;
        private readonly ObservableAsPropertyHelper<UserObject> user;
        private string userQueryString;

        // Computer
        private readonly ReactiveCommand<Unit, string> computerPaste;
        private readonly ReactiveCommand<Unit, ComputerObject> findComputer;
        private readonly ReactiveCommand<Unit, Unit> computerPasteAndFind;
        private readonly ObservableAsPropertyHelper<ComputerObject> computer;
        private string computerQueryString;



        public MainWindowViewModel()
        {
            MessageBus.Current.Listen<ApplicationActionRequest>()
                .Subscribe(a => ApplicationActionRequestImpl(a));

            // User
            userPaste = ReactiveCommand.Create(() => UserQueryString = Clipboard.GetText());

            findUser = ReactiveCommand.CreateFromObservable(
                () => ActiveDirectoryService.Current.GetUser(UserQueryString),
                this.WhenAnyValue(x => x.UserQueryString, x => x.HasValue()));
            findUser
                .ToProperty(this, x => x.User, out user);
            findUser
                .ThrownExceptions
                .Subscribe(ex => DialogService.ShowError(ex.Message));

            userPasteAndFind = ReactiveCommand.CreateFromTask(async () =>
            {
                await userPaste.Execute();
                await findUser.Execute();
            });

            // Computer
            computerPaste = ReactiveCommand.Create(() => ComputerQueryString = Clipboard.GetText());

            findComputer = ReactiveCommand.CreateFromObservable(
                () => ActiveDirectoryService.Current.GetComputer(ComputerQueryString),
                this.WhenAnyValue(x => x.ComputerQueryString, x => x.HasValue()));
            findComputer
                .ToProperty(this, x => x.Computer, out computer);
            findComputer
                .ThrownExceptions
                .Subscribe(ex => DialogService.ShowError(ex.Message));

            computerPasteAndFind = ReactiveCommand.CreateFromTask(async () =>
            {
                await computerPaste.Execute();
                await findComputer.Execute();
            });
        }



        // User
        public ReactiveCommand UserPaste => userPaste;

        public ReactiveCommand<Unit, UserObject> FindUser => findUser;

        public ReactiveCommand UserPasteAndFind => userPasteAndFind;

        public UserObject User => user.Value;

        public string UserQueryString
        {
            get { return userQueryString; }
            set { this.RaiseAndSetIfChanged(ref userQueryString, value); }
        }

        // Computer
        public ReactiveCommand ComputerPaste => computerPaste;

        public ReactiveCommand<Unit, ComputerObject> FindComputer => findComputer;

        public ReactiveCommand ComputerPasteAndFind => computerPasteAndFind;

        public ComputerObject Computer => computer.Value;

        public string ComputerQueryString
        {
            get { return computerQueryString; }
            set { this.RaiseAndSetIfChanged(ref computerQueryString, value); }
        }



        private async void ApplicationActionRequestImpl(ApplicationActionRequest a)
        {
            switch (a)
            {
                case ApplicationActionRequest.RefreshUser:
                    await FindUser.Execute();
                    break;
                default:
                    break;
            }
        }



        public Task OnNavigatedTo(object parameter) => Task.FromResult<object>(null);

        public Task OnNavigatingFrom() => Task.FromResult<object>(null);
    }
}
