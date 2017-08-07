using ReactiveUI;
using MagnumOpus.Models;
using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Concurrency;

namespace MagnumOpus.ViewModels
{
	public class PingPanelViewModel : ViewModelBase
	{
		public PingPanelViewModel()
		{
			StartPing = ReactiveCommand.CreateFromObservable(() =>
                {
                    PingResults.Clear();
                    return PingHost(_hostName).TakeUntil(StopPing);
                });

			StopPing = ReactiveCommand.Create(() => Unit.Default);

            _mostRecentPingResult = Observable.Merge(
				_pingResults.ItemsAdded,
				StopPing.Select(_ => ""),
				this.WhenAnyValue(vm => vm.HostName).WhereNotNull().Select(_ => ""))
				.ToProperty(this, vm => vm.MostRecentPingResult);

            this.WhenActivated(disposables =>
            {
                StartPing
                    .Do(pingMessage => PingResults.Insert(0, pingMessage))
                    .Subscribe()
                    .DisposeWith(disposables);

                this
                    .WhenAnyValue(vm => vm.IsPinging)
                    .Where(false)
                    .Subscribe(_ => IsShowingPingResultDetails = false)
                    .DisposeWith(disposables);

                Observable.Merge<(string Title, string Message)>(
                        StartPing.ThrownExceptions.Select(ex => (("Could not start pinging", ex.Message))),
                        StopPing.ThrownExceptions.Select(ex => (("Could not stop pinging", ex.Message))))
                    .SelectMany(dialogContent => _messages.Handle(new MessageInfo(MessageType.Error, dialogContent.Message, dialogContent.Title)))
                    .Subscribe()
                    .DisposeWith(disposables);
            });
		}



		public ReactiveCommand<Unit, string> StartPing { get; private set; }
		public ReactiveCommand<Unit, Unit> StopPing { get; private set; }
        public ReactiveList<string> PingResults => _pingResults;
		public string MostRecentPingResult => _mostRecentPingResult.Value;
        public string HostName { get => _hostName; set => this.RaiseAndSetIfChanged(ref _hostName, value); }
        public bool IsPinging { get => _isPinging; set => this.RaiseAndSetIfChanged(ref _isPinging, value); }
        public bool IsShowingPingResultDetails { get => _isShowingPingResultDetails; set => this.RaiseAndSetIfChanged(ref _isShowingPingResultDetails, value); }



        private IObservable<string> PingHost(string hostName) => Observable.Interval(TimeSpan.FromSeconds(2d), TaskPoolScheduler.Default)
                .SelectMany(_ => Observable.Start(() =>
                    {
                        PingReply reply = null;
                        reply = new Ping().Send(hostName, 1000);

                        if (reply?.Status == IPStatus.Success) return $"{hostName} responded after {reply.RoundtripTime}ms";
                        else throw new Exception("Ping reply status was not 'Success'");
                    }, CurrentThreadScheduler.Instance)
                    .CatchAndReturn($"{hostName} did not respond")
                    .Select(pingMessage => $"{DateTimeOffset.Now.ToString("T")} - {pingMessage}"))
                .StartWith($"{DateTimeOffset.Now.ToString("T")} - Waiting for {hostName} to respond...");



        private readonly ReactiveList<string> _pingResults = new ReactiveList<string>();
        private readonly ObservableAsPropertyHelper<string> _mostRecentPingResult;
        private string _hostName;
        private bool _isPinging;
        private bool _isShowingPingResultDetails;
    }
}
