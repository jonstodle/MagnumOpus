using ReactiveUI;
using System;
using System.Net;
using System.Reactive.Linq;

namespace MagnumOpus.IPAddress
{
    public class IPAddressPanelViewModel : ViewModelBase
	{
		public IPAddressPanelViewModel()
		{
			_computerName = this.WhenAnyValue(vm => vm.IPAddress)
				.Where(ipAddress => ipAddress.HasValue())
				.Select(ipAddress => Dns.GetHostEntry(ipAddress).HostName)
				.CatchAndReturn("")
				.ToProperty(this, vm => vm.ComputerName);
		}



		public string ComputerName => _computerName.Value;
        public string IPAddress { get => _ipAddress; set => this.RaiseAndSetIfChanged(ref _ipAddress, value); }



        private readonly ObservableAsPropertyHelper<string> _computerName;
        private string _ipAddress;
    }
}
