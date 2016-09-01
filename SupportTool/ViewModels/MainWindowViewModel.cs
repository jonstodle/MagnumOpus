using ReactiveUI;
using SupportTool.Helpers;
using SupportTool.Models;
using SupportTool.Services.ActiveDirectoryServices;
using SupportTool.Services.DialogServices;
using SupportTool.Services.NavigationServices;
using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
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
        private readonly ReactiveCommand<Unit, Unit> _findPreviousIdenitity;
        private readonly ReactiveCommand<Unit, Principal> find;
        private readonly ReactiveCommand<Unit, string> pasteAndFind;
        private readonly ReactiveList<string> _previousIdentities;
        private UserObject user;
        private ComputerObject computer;
        private string queryString;



        public MainWindowViewModel()
        {
            _previousIdentities = new ReactiveList<string>();

            MessageBus.Current.Listen<ApplicationActionRequest>()
                .Subscribe(a => ApplicationActionRequestImpl(a));

            _findPreviousIdenitity = ReactiveCommand.CreateFromTask(
                async () =>
                {
                    QueryString = _previousIdentities.Reverse().Skip(1).First();
                    _previousIdentities.RemoveRange(_previousIdentities.Count - 2, 2);
                    await find.Execute();
                },
                this.WhenAnyObservable(x => x._previousIdentities.CountChanged).Select(x => x > 1));

            find = ReactiveCommand.CreateFromObservable(
                () => ActiveDirectoryService.Current.GetPrincipal(QueryString).SubscribeOn(RxApp.TaskpoolScheduler),
                this.WhenAnyValue(x => x.QueryString, x => x.HasValue()));
            find
                .ObserveOnDispatcher()
                .Subscribe(x =>
                {
                    User = null;
                    Computer = null;

                    if (x is UserPrincipal)
                    {
                        User = new UserObject(x as UserPrincipal);
                        AddToPreviousIdentities(User.Principal.SamAccountName);
                    }
                    else if (x is ComputerPrincipal)
                    {
                        Computer = new ComputerObject(x as ComputerPrincipal);
                        AddToPreviousIdentities(Computer.CN);
                    }
                });
            find
                .ThrownExceptions
                .Subscribe(ex => DialogService.ShowError(ex.Message));

            pasteAndFind = ReactiveCommand.Create(() => QueryString = Clipboard.GetText());
            pasteAndFind
                .InvokeCommand(Find);
        }



        public ReactiveCommand FindPreviousIdentity => _findPreviousIdenitity;

        public ReactiveCommand<Unit, Principal> Find => find;

        public ReactiveCommand PasteAndFind => pasteAndFind;

        public ReactiveList<string> PreviousIdentities => _previousIdentities;

        public UserObject User
        {
            get { return user; }
            set { this.RaiseAndSetIfChanged(ref user, value); }
        }

        public ComputerObject Computer
        {
            get { return computer; }
            set { this.RaiseAndSetIfChanged(ref computer, value); }
        }

        public string QueryString
        {
            get { return queryString; }
            set { this.RaiseAndSetIfChanged(ref queryString, value); }
        }



        private async void ApplicationActionRequestImpl(ApplicationActionRequest a)
        {
            switch (a)
            {
                case ApplicationActionRequest.Refresh:
                    await Find.Execute();
                    break;
                default:
                    break;
            }
        }

        private void AddToPreviousIdentities(string item)
        {
            if (item != (_previousIdentities.LastOrDefault() ?? "")) _previousIdentities.Add(item); 
        }



        public Task OnNavigatedTo(object parameter) => Task.FromResult<object>(null);

        public Task OnNavigatingFrom() => Task.FromResult<object>(null);
    }
}
