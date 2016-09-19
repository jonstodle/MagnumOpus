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
	public class ComputerWindowViewModel : ReactiveObject, INavigable
	{
		private ComputerObject _computer;



		public ComputerWindowViewModel()
		{

		}



		public ComputerObject Computer
		{
			get { return _computer; }
			set { this.RaiseAndSetIfChanged(ref _computer, value); }
		}



		public async Task OnNavigatedTo(object parameter)
		{
			if (parameter is string)
			{
				Computer = await ActiveDirectoryService.Current.GetComputer(parameter as string);
			}
		}

		public Task OnNavigatingFrom() => Task.FromResult<object>(null);
	}
}
