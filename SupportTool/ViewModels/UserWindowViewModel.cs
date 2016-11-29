﻿using ReactiveUI;
using SupportTool.Models;
using SupportTool.Services.ActiveDirectoryServices;
using SupportTool.Services.NavigationServices;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace SupportTool.ViewModels
{
	public class UserWindowViewModel : ViewModelBase, INavigable
	{
		private readonly ReactiveCommand<string, UserObject> _setUser;
		private readonly ObservableAsPropertyHelper<UserObject> _user;



		public UserWindowViewModel()
		{
			_setUser = ReactiveCommand.CreateFromObservable<string, UserObject>(identity => ActiveDirectoryService.Current.GetUser(identity));
			_setUser
				.ToProperty(this, x => x.User, out _user);
			_setUser
				.ThrownExceptions
				.Subscribe(async ex => await _errorMessages.Handle(new MessageInfo(ex.Message)));
		}



		public ReactiveCommand SetUser => _setUser;

		public UserObject User => _user.Value;



		public Task OnNavigatedTo(object parameter)
		{
			if (parameter is string)
			{
				Observable.Return(parameter as string)
					.InvokeCommand(_setUser);
			}

			return Task.FromResult<object>(null);
		}

		public Task OnNavigatingFrom() => Task.FromResult<object>(null);
	}
}
