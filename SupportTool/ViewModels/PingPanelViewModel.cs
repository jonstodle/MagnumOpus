using ReactiveUI;
using SupportTool.Models;
using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reactive;
using System.Reactive.Linq;

namespace SupportTool.ViewModels
{
	public class PingPanelViewModel : ViewModelBase
	{
		private readonly ReactiveCommand<Unit, string> _startPing;
		private readonly ReactiveCommand<Unit, Unit> _stopPing;
		private readonly ReactiveList<string> _pingResults;
		private readonly ObservableAsPropertyHelper<string> _mostRecentPingResult;
		private string _hostName;
		private bool _isPinging;
		private bool _isShowingPingResultDetails;



		public PingPanelViewModel()
		{
			_pingResults = new ReactiveList<string>();

			_startPing = ReactiveCommand.CreateFromObservable(() =>
				{
					PingResults.Clear();
					return PingHost(_hostName).TakeUntil(_stopPing);
				});
			_startPing
				.Subscribe(x => PingResults.Insert(0, x));
			_startPing
				.ThrownExceptions
				.Subscribe(async ex => await _errorMessages.Handle(new MessageInfo(ex.Message)));

			_stopPing = ReactiveCommand.Create(() => Unit.Default);

			Observable.Merge(
				_pingResults.ItemsAdded,
				_stopPing.Select(_ => ""),
				this.WhenAnyValue(x => x.HostName).WhereNotNull().Select(_ => ""))
				.ToProperty(this, x => x.MostRecentPingResult, out _mostRecentPingResult);

			this
				.WhenAnyValue(x => x.IsPinging)
				.Where(x => !x)
				.Subscribe(_ => IsShowingPingResultDetails = false);

			Observable.Merge(
				_startPing.ThrownExceptions,
				_stopPing.ThrownExceptions)
				.Subscribe(async ex => await _errorMessages.Handle(new MessageInfo(ex.Message)));
		}



		public ReactiveCommand StartPing => _startPing;

		public ReactiveCommand StopPing => _stopPing;

		public ReactiveList<string> PingResults => _pingResults;

		public string MostRecentPingResult => _mostRecentPingResult.Value;

		public string HostName
		{
			get { return _hostName; }
			set { this.RaiseAndSetIfChanged(ref _hostName, value); }
		}

		public bool IsPinging
		{
			get { return _isPinging; }
			set { this.RaiseAndSetIfChanged(ref _isPinging, value); }
		}

		public bool IsShowingPingResultDetails
		{
			get { return _isShowingPingResultDetails; }
			set { this.RaiseAndSetIfChanged(ref _isShowingPingResultDetails, value); }
		}



        private IObservable<string> PingHost(string hostName) => Observable.Interval(TimeSpan.FromSeconds(2d))
                .SelectMany(_ => Observable.Start(() =>
                    {
                        PingReply reply = null;
                        reply = new Ping().Send(hostName, 1000);

                        if (reply?.Status == IPStatus.Success) return $"{hostName} responded after {reply.RoundtripTime}ms";
                        else throw new Exception("Ping reply status was not 'Success'");
                    })
                    .Catch(Observable.Return($"{hostName} did not respond"))
                    .Select(x => $"{DateTimeOffset.Now.ToString("T")} - {x}"))
                .StartWith($"{DateTimeOffset.Now.ToString("T")} - Waiting for {hostName} to respond...");
	}
}
