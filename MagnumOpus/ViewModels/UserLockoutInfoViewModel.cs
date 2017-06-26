using ReactiveUI;
using MagnumOpus.Models;
using MagnumOpus.Services.ActiveDirectoryServices;
using MagnumOpus.Services.NavigationServices;
using System;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace MagnumOpus.ViewModels
{
    public class UserLockoutInfoViewModel : ViewModelBase, IDialog
	{
		public UserLockoutInfoViewModel()
		{
			SetUser = ReactiveCommand.CreateFromObservable<string, UserObject>(x => ActiveDirectoryService.Current.GetUser(x, RxApp.TaskpoolScheduler));

			GetLockoutInfo = ReactiveCommand.Create<Unit, IObservable<LockoutInfo>>(_ => ActiveDirectoryService.Current.GetLockoutInfo(_user.Value.CN, RxApp.TaskpoolScheduler));

			Close = ReactiveCommand.Create(_closeAction);

			_user = SetUser
				.ToProperty(this, x => x.User);

            this.WhenActivated(disposables =>
            {
                GetLockoutInfo
                    .Do((IObservable<LockoutInfo> _) => _lockoutInfos.Clear())
                    .Switch()
                    .ObserveOnDispatcher()
                    .Subscribe(x => _lockoutInfos.Add(x))
                    .DisposeWith(disposables);

                Observable.Merge(
                        SetUser.ThrownExceptions.Select(ex => (("Could not load user", ex.Message))),
                        GetLockoutInfo.ThrownExceptions.Select(ex => (("Could not get lockout info", ex.Message))),
                        Close.ThrownExceptions.Select(ex => (("Could not close dialog", ex.Message))))
                    .SelectMany(((string, string) x) => _messages.Handle(new MessageInfo(MessageType.Error, x.Item2, x.Item1)))
                    .Subscribe()
                    .DisposeWith(disposables);
            });
		}



		public ReactiveCommand<string, UserObject> SetUser { get; private set; }
        public ReactiveCommand<Unit, IObservable<LockoutInfo>> GetLockoutInfo { get; private set; }
        public ReactiveCommand<Unit, Unit> Close { get; private set; }
        public ReactiveList<LockoutInfo> LockoutInfos => _lockoutInfos;
		public UserObject User => _user.Value;



		public Task Opening(Action close, object parameter)
		{
			_closeAction = close;

			if (parameter is string s)
			{
				Observable.Return(s)
					.InvokeCommand(SetUser);
			}

			return Task.FromResult<object>(null);
		}



		private readonly ReactiveList<LockoutInfo> _lockoutInfos = new ReactiveList<LockoutInfo>();
		private readonly ObservableAsPropertyHelper<UserObject> _user;
		private Action _closeAction;
	}
}
