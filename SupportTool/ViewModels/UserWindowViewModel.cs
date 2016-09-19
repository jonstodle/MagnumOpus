using ReactiveUI;
using SupportTool.Models;
using SupportTool.Services.ActiveDirectoryServices;
using SupportTool.Services.NavigationServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupportTool.ViewModels
{
	public class UserWindowViewModel : ReactiveObject, INavigable
	{
		private readonly ReactiveCommand<string, UserObject> _setUser;
		private readonly ObservableAsPropertyHelper<UserObject> _user;



		public UserWindowViewModel()
		{
			_setUser = ReactiveCommand.CreateFromObservable<string, UserObject>(identity => ActiveDirectoryService.Current.GetUser(identity));
			_setUser
				.ToProperty(this, x => x.User, out _user);
		}



		public ReactiveCommand SetUser => _setUser;

		public UserObject User => _user.Value;



		public async Task OnNavigatedTo(object parameter)
		{
			if (parameter is string)
			{
				Observable.Return(parameter as string)
					.InvokeCommand(_setUser);
			}
		}

		public Task OnNavigatingFrom() => Task.FromResult<object>(null);
	}
}
