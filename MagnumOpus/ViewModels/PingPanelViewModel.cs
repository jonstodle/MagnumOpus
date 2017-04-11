using ReactiveUI;
using MagnumOpus.Models;
using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace MagnumOpus.ViewModels
{
	public class PingPanelViewModel : ViewModelBase
	{
		public PingPanelViewModel()
		{
			_startPing = ReactiveCommand.CreateFromObservable(() =>
				{
					PingResults.Clear();
					return PingHost(_hostName).TakeUntil(_stopPing);
				});

			_stopPing = ReactiveCommand.Create(() => Unit.Default);

            _mostRecentPingResult = Observable.Merge(
				_pingResults.ItemsAdded,
				_stopPing.Select(_ => ""),
				this.WhenAnyValue(x => x.HostName).WhereNotNull().Select(_ => ""))
				.ToProperty(this, x => x.MostRecentPingResult);

            this.WhenActivated(disposables =>
            {
                _startPing
                    .Subscribe(x => PingResults.Insert(0, x))
                    .DisposeWith(disposables);

                _startPing
                    .ThrownExceptions
                    .SelectMany(ex => _messages.Handle(new MessageInfo(MessageType.Error, ex.Message)))
                    .Subscribe()
                    .DisposeWith(disposables);

                this
                .WhenAnyValue(x => x.IsPinging)
                .Where(x => !x)
                .Subscribe(_ => IsShowingPingResultDetails = false)
                .DisposeWith(disposables);

                Observable.Merge(
                    _startPing.ThrownExceptions,
                    _stopPing.ThrownExceptions)
                    .SelectMany(ex => _messages.Handle(new MessageInfo(MessageType.Error, ex.Message)))
                    .Subscribe()
                    .DisposeWith(disposables);
            });
		}



		public ReactiveCommand StartPing => _startPing;

		public ReactiveCommand StopPing => _stopPing;

		public ReactiveList<string> PingResults => _pingResults;

		public string MostRecentPingResult => _mostRecentPingResult.Value;

        public string HostName { get => _hostName; set => this.RaiseAndSetIfChanged(ref _hostName, value); }

        public bool IsPinging { get => _isPinging; set => this.RaiseAndSetIfChanged(ref _isPinging, value); }

        public bool IsShowingPingResultDetails { get => _isShowingPingResultDetails; set => this.RaiseAndSetIfChanged(ref _isShowingPingResultDetails, value); }



        private IObservable<string> PingHost(string hostName) => Observable.Interval(TimeSpan.FromSeconds(2d))
                .SelectMany(_ => Observable.Start(() =>
                    {
                        PingReply reply = null;
                        reply = new Ping().Send(hostName, 1000);

                        if (reply?.Status == IPStatus.Success) return $"{hostName} responded after {reply.RoundtripTime}ms";
                        else throw new Exception("Ping reply status was not 'Success'");
                    })
                    .CatchAndReturn($"{hostName} did not respond")
                    .Select(x => $"{DateTimeOffset.Now.ToString("T")} - {x}"))
                .StartWith($"{DateTimeOffset.Now.ToString("T")} - Waiting for {hostName} to respond...");



        private readonly ReactiveCommand<Unit, string> _startPing;
        private readonly ReactiveCommand<Unit, Unit> _stopPing;
        private readonly ReactiveList<string> _pingResults = new ReactiveList<string>();
        private readonly ObservableAsPropertyHelper<string> _mostRecentPingResult;
        private string _hostName;
        private bool _isPinging;
        private bool _isShowingPingResultDetails;
    }
}
