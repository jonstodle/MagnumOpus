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
        private readonly ReactiveCommand<Unit, Unit> _navigateBack;
        private readonly ReactiveCommand<Unit, Unit> _navigateForward;
        private readonly ReactiveCommand<Unit, Unit> _find;
        private readonly ReactiveCommand<Unit, Unit> _pasteAndFind;
        private readonly ReactiveCommand<Unit, Principal> _open;
        private readonly ReactiveList<string> _navigationStack;
        private UserObject _user;
        private ComputerObject _computer;
        private GroupObject _group;
        private string _queryString;
        private int _currentNavigationIndex;
        private ObservableAsPropertyHelper<IReadOnlyList<string>> _reverseNavigationStack;



        public MainWindowViewModel()
        {
            _navigationStack = new ReactiveList<string>();
            _currentNavigationIndex = -1;

            MessageBus.Current.Listen<ApplicationActionRequest>()
                .Subscribe(a => ApplicationActionRequestImpl(a));

            _navigateBack = ReactiveCommand.Create(
                () =>
                {
                    CurrentNavigationIndex -= 1;
                    QueryString = _navigationStack[_currentNavigationIndex];
                },
                Observable.CombineLatest(
                    this.WhenAnyValue(x => x.CurrentNavigationIndex),
                    this.WhenAnyObservable(x => x._navigationStack.CountChanged),
                    (x, y) => x > 0 && x <= y));

            _navigateForward = ReactiveCommand.Create(
                () =>
                {
                    CurrentNavigationIndex += 1;
                    QueryString = _navigationStack[_currentNavigationIndex];
                },
                Observable.CombineLatest(
                    this.WhenAnyValue(x => x.CurrentNavigationIndex),
                    this.WhenAnyObservable(x => x._navigationStack.CountChanged),
                    (x, y) => x >= -1 && x < (y - 1)));

            _find = ReactiveCommand.Create(
                () =>
                {
                    if (_currentNavigationIndex < (_navigationStack.Count - 1)) _navigationStack.RemoveRange(CurrentNavigationIndex + 1, _navigationStack.Count - (_currentNavigationIndex + 1));
                    _navigationStack.Add(QueryString);
                },
                this.WhenAnyValue(x => x.QueryString, x => x.HasValue()));
            _find
                .ObserveOnDispatcher()
                .InvokeCommand(_navigateForward);
            _find
                .ThrownExceptions
                .Subscribe(ex => DialogService.ShowError(ex.Message));

            _pasteAndFind = ReactiveCommand.Create(() => { QueryString = Clipboard.GetText()?.ToUpperInvariant(); });
            _pasteAndFind
                .InvokeCommand(Find);

            _open = ReactiveCommand.CreateFromObservable(() =>
            {
                return ActiveDirectoryService.Current.GetPrincipal(_queryString).SubscribeOn(RxApp.TaskpoolScheduler);
            });
            _open
                .NotNull()
                .ObserveOnDispatcher()
                .Subscribe(x =>
                {
                    User = null;
                    Computer = null;

                    if (x is UserPrincipal) User = new UserObject(x as UserPrincipal);
                    else if (x is ComputerPrincipal) Computer = new ComputerObject(x as ComputerPrincipal);
                    else if (x is GroupPrincipal) Group = new GroupObject(x as GroupPrincipal);
                });
            _open
                .Where(x => x == null)
                .ObserveOnDispatcher()
                .Do(_ => _navigationStack.Remove(_navigationStack.LastOrDefault()))
                .Select(_ => Unit.Default)
                .InvokeCommand(_navigateBack);

            _reverseNavigationStack = this
                .WhenAnyObservable(x => x._navigationStack.Changed)
                .Select(_ => _navigationStack.Reverse().ToList())
                .ToProperty(this, x => x.ReverseNavigationStack, new List<string>());

            Observable.Merge(
                _navigateBack,
                _navigateForward)
                .InvokeCommand(_open);

            Observable.Merge(
                _navigateBack.ThrownExceptions,
                _navigateForward.ThrownExceptions,
                _find.ThrownExceptions,
                _pasteAndFind.ThrownExceptions,
                _open.ThrownExceptions)
                .Subscribe(ex => DialogService.ShowError(ex.Message));
        }



        public ReactiveCommand NavigateBack => _navigateBack;

        public ReactiveCommand NavigateForward => _navigateForward;

        public ReactiveCommand Find => _find;

        public ReactiveCommand PasteAndFind => _pasteAndFind;

        public ReactiveCommand Open => _open;

        public ReactiveList<string> NavigationStack => _navigationStack;

        public IReadOnlyList<string> ReverseNavigationStack => _reverseNavigationStack.Value;

        public UserObject User
        {
            get { return _user; }
            set { this.RaiseAndSetIfChanged(ref _user, value); }
        }

        public ComputerObject Computer
        {
            get { return _computer; }
            set { this.RaiseAndSetIfChanged(ref _computer, value); }
        }

        public GroupObject Group
        {
            get { return _group; }
            set { this.RaiseAndSetIfChanged(ref _group, value); }
        }

        public string QueryString
        {
            get { return _queryString; }
            set { this.RaiseAndSetIfChanged(ref _queryString, value); }
        }

        public int CurrentNavigationIndex
        {
            get { return _currentNavigationIndex; }
            set { this.RaiseAndSetIfChanged(ref _currentNavigationIndex, value); }
        }



        private async void ApplicationActionRequestImpl(ApplicationActionRequest a)
        {
            switch (a)
            {
                case ApplicationActionRequest.Refresh:
                    await _open.Execute();
                    break;
                default:
                    break;
            }
        }

        private void AddToPreviousIdentities(string item)
        {
            if (item != (_navigationStack.LastOrDefault() ?? "")) _navigationStack.Add(item);
        }



        public Task OnNavigatedTo(object parameter) => Task.FromResult<object>(null);

        public Task OnNavigatingFrom() => Task.FromResult<object>(null);
    }
}
