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
		private UserObject _user;



		public UserWindowViewModel()
		{

		}



		public UserObject User
		{
			get { return _user; }
			set { this.RaiseAndSetIfChanged(ref _user, value); }
		}



		public async Task OnNavigatedTo(object parameter)
		{
			if (parameter is string)
			{
				User = await ActiveDirectoryService.Current.GetUser(parameter as string);
			}
		}

		public Task OnNavigatingFrom() => Task.FromResult<object>(null);
	}
}
