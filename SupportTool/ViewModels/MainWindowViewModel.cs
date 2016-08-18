using ReactiveUI;
using SupportTool.Helpers;
using SupportTool.Models;
using SupportTool.Services.ActiveDirectoryServices;
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
        private readonly Subject<Message> messages;

        // User
        private readonly ReactiveCommand<Unit, string> userPaste;
        private readonly ReactiveCommand<Unit, UserObject> findUser;
        private readonly ObservableAsPropertyHelper<UserObject> user;
        private string userQueryString;

        // Computer
        private readonly ReactiveCommand<Unit, string> computerPaste;
        private readonly ReactiveCommand<Unit, ComputerObject> findComputer;
        private readonly ObservableAsPropertyHelper<ComputerObject> computer;
        private string computerQueryString;



        public MainWindowViewModel()
        {
            messages = new Subject<Message>();
            MessageBus.Current.Listen<ApplicationActionRequest>()
                .Subscribe(a => ApplicationActionRequestImpl(a));

            // User
            userPaste = ReactiveCommand.Create(() => Clipboard.GetText());
            userPaste
                .Subscribe(text => UserQueryString = text);

            findUser = ReactiveCommand.CreateFromObservable(
                () => ActiveDirectoryService.Current.GetUser(UserQueryString),
                this.WhenAnyValue(x => x.UserQueryString, x => x.HasValue()));
            findUser
                .ToProperty(this, x => x.User, out user);

            // Computer
            computerPaste = ReactiveCommand.Create(() => Clipboard.GetText());
            computerPaste
                .Subscribe(text => ComputerQueryString = text);

            findComputer = ReactiveCommand.CreateFromObservable(
                () => ActiveDirectoryService.Current.GetComputer(ComputerQueryString),
                this.WhenAnyValue(x => x.ComputerQueryString, x => x.HasValue()));
            findComputer
                .ToProperty(this, x => x.Computer, out computer);
        }



        // User
        public IObservable<Message> Messages => messages;

        public ReactiveCommand UserPaste => userPaste;

        public ReactiveCommand<Unit, UserObject> FindUser => findUser;

        public UserObject User => user.Value;

        public string UserQueryString
        {
            get { return userQueryString; }
            set { this.RaiseAndSetIfChanged(ref userQueryString, value); }
        }

        // Computer
        public ReactiveCommand<Unit, string> ComputerPaste => computerPaste;

        public ReactiveCommand<Unit, ComputerObject> FindComputer => findComputer;

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
