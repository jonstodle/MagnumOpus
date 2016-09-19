using ReactiveUI;
using SupportTool.Services.NavigationServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupportTool.ViewModels
{
	public class IPAddressWindowViewModel : ReactiveObject, INavigable
	{
		private string _ipAddress;



		public IPAddressWindowViewModel()
		{

		}



		public string IPAddress
		{
			get { return _ipAddress; }
			set { this.RaiseAndSetIfChanged(ref _ipAddress, value); }
		}



		public Task OnNavigatedTo(object parameter)
		{
			if (parameter is string)
			{
				IPAddress = parameter as string;
			}

			return Task.FromResult<object>(null);
		}

		public Task OnNavigatingFrom() => Task.FromResult<object>(null);
	}
}
