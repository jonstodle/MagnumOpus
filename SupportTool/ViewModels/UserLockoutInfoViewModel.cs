using ReactiveUI;
using SupportTool.Models;
using SupportTool.Services.ActiveDirectoryServices;
using SupportTool.Services.NavigationServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupportTool.ViewModels
{
	public class UserLockoutInfoViewModel : ViewModelBase, IDialog
	{
		public UserLockoutInfoViewModel()
		{
			_setUser = ReactiveCommand.CreateFromObservable<string, UserObject>(x => ActiveDirectoryService.Current.GetUser(x).SubscribeOn(TaskPoolScheduler.Default));

			_getLockoutInfo = ReactiveCommand.Create<Unit, IObservable<LockoutInfo>>(_ => ActiveDirectoryService.Current.GetLockoutInfo(_user.Value.CN).SubscribeOn(TaskPoolScheduler.Default));
			_getLockoutInfo
				.Do(_ => _lockoutInfos.Clear())
				.Switch()
				.ObserveOnDispatcher()
				.Subscribe(x => _lockoutInfos.Add(x));

			_close = ReactiveCommand.Create(_closeAction);

			_user = _setUser
				.ToProperty(this, x => x.User);

			Observable.Merge(
				_setUser.ThrownExceptions,
				_getLockoutInfo.ThrownExceptions,
				_close.ThrownExceptions)
				.Subscribe(async ex => await _errorMessages.Handle(new MessageInfo(ex.Message)));
		}



		public ReactiveCommand<string, UserObject> SetUser => _setUser;

		public ReactiveCommand GetLockoutInfo => _getLockoutInfo;

		public ReactiveCommand Close => _close;

		public ReactiveList<LockoutInfo> LockoutInfos => _lockoutInfos;

		public UserObject User => _user.Value;



		public Task Opening(Action close, object parameter)
		{
			_closeAction = close;

			if (parameter is string s)
			{
				Observable.Return(s)
					.InvokeCommand(_setUser);
			}

			return Task.FromResult<object>(null);
		}



		private readonly ReactiveCommand<string, UserObject> _setUser;
		private readonly ReactiveCommand<Unit, IObservable<LockoutInfo>> _getLockoutInfo;
		private readonly ReactiveCommand<Unit, Unit> _close;
		private readonly ReactiveList<LockoutInfo> _lockoutInfos = new ReactiveList<LockoutInfo>();
		private readonly ObservableAsPropertyHelper<UserObject> _user;
		private Action _closeAction;
	}
}
