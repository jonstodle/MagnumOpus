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
            ToggleIsShowingDetails = ReactiveCommand.Create(() => !_isShowingDetails.Value);

            var newComputer = this.WhenAnyValue(vm => vm.Computer)
                .WhereNotNull()
                .Publish()
                .RefCount();

			_operatingSystemInfo = newComputer
				.SelectMany(computerObject => GetOSInfo(computerObject).CatchAndReturn(null))
				.ToProperty(this, vm => vm.OperatingSystemInfo, null, scheduler: DispatcherScheduler.Current);

			_ipAddress = newComputer
				.SelectMany(computerObject => computerObject.GetIPAddress())
				.ObserveOnDispatcher()
				.ToProperty(this, vm => vm.IPAddress, null);

            _isShowingDetails = ToggleIsShowingDetails
                .ToProperty(this, vm => vm.IsShowingDetails);
        }



        public ReactiveCommand<Unit, bool> ToggleIsShowingDetails { get; private set; }
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
		}, TaskPoolScheduler.Default);



        private readonly ObservableAsPropertyHelper<OperatingSystemInfo> _operatingSystemInfo;
        private readonly ObservableAsPropertyHelper<string> _ipAddress;
        private readonly ObservableAsPropertyHelper<bool> _isShowingDetails;
        private ComputerObject _computer;
    }
}
