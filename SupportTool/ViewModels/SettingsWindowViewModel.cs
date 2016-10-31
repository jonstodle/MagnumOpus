using ReactiveUI;
using SupportTool.Services.NavigationServices;
using SupportTool.Services.SettingsServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupportTool.ViewModels
{
	public class SettingsWindowViewModel : ReactiveObject, IDialog
	{
		private string _historyCountLimit = SettingsService.Current.HistoryCountLimit.ToString();
		private string _detailWindowTimeoutLength = SettingsService.Current.DetailsWindowTimeoutLength.ToString();
		private Action _close;



		public SettingsWindowViewModel()
		{
			this.WhenAnyValue(x => x.HistoryCountLimit)
				.Where(x => x.IsLong())
				.Select(x => long.Parse(x))
				.Where(x => x > 0)
				.Subscribe(x => SettingsService.Current.HistoryCountLimit = x);

			this.WhenAnyValue(x => x.DetailWindowTimeoutLength)
				.Where(x => x.IsLong())
				.Select(x => long.Parse(x))
				.Where(x => x > 0)
				.Subscribe(x => SettingsService.Current.DetailsWindowTimeoutLength = x);
		}



		public string HistoryCountLimit
		{
			get { return _historyCountLimit; }
			set { this.RaiseAndSetIfChanged(ref _historyCountLimit, value); }
		}

		public string DetailWindowTimeoutLength
		{
			get { return _detailWindowTimeoutLength; }
			set { this.RaiseAndSetIfChanged(ref _detailWindowTimeoutLength, value); }
		}

		public Task Opening(Action close, object parameter)
		{
			_close = close;
			return Task.FromResult<object>(null);
		}
	}
}
