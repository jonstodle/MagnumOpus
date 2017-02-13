using ReactiveUI;
using MagnumOpus.Models;
using System;
using System.Linq;
using System.Management;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace MagnumOpus.ViewModels
{
	public class ComputerDetailsViewModel : ViewModelBase
    {
        public ComputerDetailsViewModel()
        {
            _toggleIsShowingDetails = ReactiveCommand.Create(() => !_isShowingDetails.Value);

            var newComputer = this.WhenAnyValue(x => x.Computer)
                .WhereNotNull()
                .Publish()
                .RefCount();

			_operatingSystemInfo = newComputer
				.SelectMany(x => GetOSInfo(x).Catch(Observable.Return<OperatingSystemInfo>(null)))
				.ToProperty(this, x => x.OperatingSystemInfo, null, scheduler: DispatcherScheduler.Current);

			_ipAddress = newComputer
				.SelectMany(x => x.GetIPAddress())
				.ObserveOnDispatcher()
				.ToProperty(this, x => x.IPAddress, null);

            _isShowingDetails = _toggleIsShowingDetails
                .ToProperty(this, x => x.IsShowingDetails);
        }



        public ReactiveCommand ToggleIsShowingDetails => _toggleIsShowingDetails;

		public OperatingSystemInfo OperatingSystemInfo => _operatingSystemInfo.Value;

		public string IPAddress => _ipAddress.Value;

        public bool IsShowingDetails => _isShowingDetails.Value;

        public ComputerObject Computer { get => _computer; set => this.RaiseAndSetIfChanged(ref _computer, value); }



        private IObservable<OperatingSystemInfo> GetOSInfo(ComputerObject computer) => Observable.Start(() =>
		{
			var scope = new ManagementScope($@"\\{computer.CN}\root\cimv2");
			scope.Connect();
			using (var searcher = new ManagementObjectSearcher(scope, new ObjectQuery("SELECT * FROM Win32_OperatingSystem")))
			using (var enumerator = searcher.Get().GetEnumerator())
			{
				enumerator.MoveNext();
				return new OperatingSystemInfo((ManagementObject)enumerator.Current);
			}
		});



        private readonly ReactiveCommand<Unit, bool> _toggleIsShowingDetails;
        private readonly ObservableAsPropertyHelper<OperatingSystemInfo> _operatingSystemInfo;
        private readonly ObservableAsPropertyHelper<string> _ipAddress;
        private readonly ObservableAsPropertyHelper<bool> _isShowingDetails;
        private ComputerObject _computer;
    }
}
