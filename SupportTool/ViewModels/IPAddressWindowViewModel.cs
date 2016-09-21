using ReactiveUI;
using SupportTool.Services.NavigationServices;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace SupportTool.ViewModels
{
	public class IPAddressWindowViewModel : ReactiveObject, INavigable
	{
		private readonly ReactiveCommand<string, string> _setIPAddress;
		private readonly ObservableAsPropertyHelper<string> _ipAddress;



		public IPAddressWindowViewModel()
		{
			_setIPAddress = ReactiveCommand.Create<string, string>(ipAddress => ipAddress);
			_setIPAddress
				.ToProperty(this, x => x.IPAddress, out _ipAddress);
		}



		public ReactiveCommand SetIPAddress => _setIPAddress;

		public string IPAddress => _ipAddress.Value;



		public Task OnNavigatedTo(object parameter)
		{
			if (parameter is string)
			{
				Observable.Return(parameter as string)
					.InvokeCommand(_setIPAddress);
			}

			return Task.FromResult<object>(null);
		}

		public Task OnNavigatingFrom() => Task.FromResult<object>(null);
	}
}
