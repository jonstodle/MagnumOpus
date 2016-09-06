using ReactiveUI;
using SupportTool.Models;
using SupportTool.Services.DialogServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace SupportTool.ViewModels
{
    public class PingPanelViewModel : ReactiveObject
    {
        private readonly ReactiveCommand<Unit, string> startPing;
        private readonly ReactiveCommand<Unit, Unit> stopPing;
        private readonly ReactiveList<string> pingResults;
        private readonly ObservableAsPropertyHelper<string> mostRecentPingResult;
        private ComputerObject computer;
        private bool isPinging;
        private bool isShowingPingResultDetails;



        public PingPanelViewModel()
        {
            pingResults = new ReactiveList<string>();

            startPing = ReactiveCommand.CreateFromObservable(
                () =>
                {
                    PingResults.Clear();
                    return PingHost(Computer.CN).TakeUntil(stopPing);
                });
            startPing
                .Subscribe(x => PingResults.Insert(0, x));
            startPing
                .ThrownExceptions
                .Subscribe(ex => DialogService.ShowError(ex.Message));

            stopPing = ReactiveCommand.Create(() => { });

			Observable.Merge(
				pingResults.ItemsAdded,
				stopPing.Select(_ => ""),
				this.WhenAnyValue(x => x.Computer).Where(x => x != null).Select(_ => ""))
                .ToProperty(this, x => x.MostRecentPingResult, out mostRecentPingResult);

            this
                .WhenAnyValue(x => x.IsPinging)
                .Where(x => !x)
                .Subscribe(_ => IsShowingPingResultDetails = false);

            this
                .WhenAnyValue(x => x.Computer)
                .Subscribe(_ => ResetValues());
        }



        public ReactiveCommand StartPing => startPing;

        public ReactiveCommand StopPing => stopPing;

        public ReactiveList<string> PingResults => pingResults;

        public string MostRecentPingResult => mostRecentPingResult.Value;

        public ComputerObject Computer
        {
            get { return computer; }
            set { this.RaiseAndSetIfChanged(ref computer, value); }
        }

        public bool IsPinging
        {
            get { return isPinging; }
            set { this.RaiseAndSetIfChanged(ref isPinging, value); }
        }

        public bool IsShowingPingResultDetails
        {
            get { return isShowingPingResultDetails; }
            set { this.RaiseAndSetIfChanged(ref isShowingPingResultDetails, value); }
        }



        private IObservable<string> PingHost(string hostName) => Observable.Create<string>(observer =>
        {
            var pinger = new Ping();
            observer.OnNext($"{DateTimeOffset.Now.ToString("T")} - Waiting for {hostName} to respond...");

            var pings = Observable.Interval(TimeSpan.FromSeconds(2d))
                .Subscribe(async _ =>
                {
                    PingReply reply = null;

                    try { reply = await pinger.SendPingAsync(hostName, 1000); }
                    catch { /* Do nothing */ }

                    observer.OnNext($"{DateTimeOffset.Now.ToString("T")} - {(reply?.Status == IPStatus.Success ? $"{hostName} responded after {reply.RoundtripTime}ms" : $"{hostName} did not respond")}");
                }, 
                () => observer.OnCompleted());

            return () => pings.Dispose();
        });

        private void ResetValues()
        {
            PingResults.Clear();
            IsPinging = false;
            IsShowingPingResultDetails = false;
        }
    }
}
