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
	public class GroupWindowViewModel : ReactiveObject, INavigable
	{
		private GroupObject _group;



		public GroupWindowViewModel()
		{

		}



		public GroupObject Group
		{
			get { return _group; }
			set { this.RaiseAndSetIfChanged(ref _group, value); }
		}



		public async Task OnNavigatedTo(object parameter)
		{
			if (parameter is string)
			{
				Group = await ActiveDirectoryService.Current.GetGroup(parameter as string);
			}
		}

		public Task OnNavigatingFrom() => Task.FromResult<object>(null);
	}
}
