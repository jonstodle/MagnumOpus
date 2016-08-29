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
        private readonly ReactiveCommand<Unit, string> paste;
        private readonly ReactiveCommand<Unit, Principal> find;
        private readonly ReactiveCommand<Unit, Unit> pasteAndFind;
        private UserObject user;
        private ComputerObject computer;
        private string queryString;



        public MainWindowViewModel()
        {
            MessageBus.Current.Listen<ApplicationActionRequest>()
                .Subscribe(a => ApplicationActionRequestImpl(a));

            paste = ReactiveCommand.Create(() => QueryString = Clipboard.GetText());

            find = ReactiveCommand.CreateFromObservable(
                () => ActiveDirectoryService.Current.GetPrincipal(QueryString),
                this.WhenAnyValue(x => x.QueryString, x => x.HasValue()));
            find
                .Subscribe(x =>
                {
                    User = null;
                    Computer = null;

                    if (x is UserPrincipal) User = new UserObject(x as UserPrincipal);
                    else if (x is ComputerPrincipal) Computer = new ComputerObject(x as ComputerPrincipal);
                });
            find
                .ThrownExceptions
                .Subscribe(ex => DialogService.ShowError(ex.Message));

            pasteAndFind = ReactiveCommand.CreateFromTask(async () =>
            {
                await paste.Execute();
                await find.Execute();
            });
        }



        public ReactiveCommand Paste => paste;

        public ReactiveCommand<Unit, Principal> Find => find;

        public ReactiveCommand PasteAndFind => pasteAndFind;

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
                case ApplicationActionRequest.RefreshUser:
                    await Find.Execute();
                    break;
                default:
                    break;
            }
        }



        public Task OnNavigatedTo(object parameter) => Task.FromResult<object>(null);

        public Task OnNavigatingFrom() => Task.FromResult<object>(null);
    }
}
