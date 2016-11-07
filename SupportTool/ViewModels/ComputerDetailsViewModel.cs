using ReactiveUI;
using SupportTool.Models;
using System;
using System.Management;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace SupportTool.ViewModels
{
	public class ComputerDetailsViewModel : ReactiveObject
    {
		private readonly ObservableAsPropertyHelper<OperatingSystemInfo> _operatingSystemInfo;
		private ComputerObject _computer;



        public ComputerDetailsViewModel()
        {
			_operatingSystemInfo = this.WhenAnyValue(x => x.Computer)
				.WhereNotNull()
				.SelectMany(x => GetOSInfo(x).Catch(Observable.Return<OperatingSystemInfo>(null)))
				.ToProperty(this, x => x.OperatingSystemInfo, null, scheduler: DispatcherScheduler.Current);
        }



		public OperatingSystemInfo OperatingSystemInfo => _operatingSystemInfo.Value;

        public ComputerObject Computer
        {
            get { return _computer; }
            set { this.RaiseAndSetIfChanged(ref _computer, value); }
        }



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
    }
}
