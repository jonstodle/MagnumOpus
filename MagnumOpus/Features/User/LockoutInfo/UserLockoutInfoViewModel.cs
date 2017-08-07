using ReactiveUI;
using System;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using MagnumOpus.ActiveDirectory;
using MagnumOpus.Dialog;

namespace MagnumOpus.User
{
    public class UserLockoutInfoViewModel : ViewModelBase, IDialog
	{
		public UserLockoutInfoViewModel()
		{
			SetUser = ReactiveCommand.CreateFromObservable<string, UserObject>(username => ActiveDirectoryService.Current.GetUser(username, TaskPoolScheduler.Default));

			GetLockoutInfo = ReactiveCommand.Create<Unit, IObservable<LockoutInfo>>(_ => ActiveDirectoryService.Current.GetLockoutInfo(_user.Value.CN, TaskPoolScheduler.Default));

			Close = ReactiveCommand.Create(_closeAction);

			_user = SetUser
				.ToProperty(this, vm => vm.User);

            this.WhenActivated(disposables =>
            {
                GetLockoutInfo
                    .Do(_ => _lockoutInfos.Clear())
                    .Switch()
                    .ObserveOnDispatcher()
                    .Subscribe(lockoutInfo => _lockoutInfos.Add(lockoutInfo))
                    .DisposeWith(disposables);

                Observable.Merge<(string Title, string Message)>(
                        SetUser.ThrownExceptions.Select(ex => ("Could not load user", ex.Message)),
                        GetLockoutInfo.ThrownExceptions.Select(ex => ("Could not get lockout info", ex.Message)),
                        Close.ThrownExceptions.Select(ex => ("Could not close dialog", ex.Message)))
                    .SelectMany(dialogContent => _messages.Handle(new MessageInfo(MessageType.Error, dialogContent.Message, dialogContent.Title)))
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
