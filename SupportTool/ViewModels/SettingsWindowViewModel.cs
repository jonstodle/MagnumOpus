using ReactiveUI;
using SupportTool.Services.NavigationServices;
using SupportTool.Services.SettingsServices;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace SupportTool.ViewModels
{
	public class SettingsWindowViewModel : ViewModelBase, IDialog
	{
		private string _historyCountLimit = SettingsService.Current.HistoryCountLimit.ToString();
		private string _detailWindowTimeoutLength = SettingsService.Current.DetailsWindowTimeoutLength.ToString();
		private string _hfName;
		private object _selectedRemoteControl2012HF;
		private Action _close;



		public SettingsWindowViewModel()
		{
			this.WhenAnyValue(x => x.HistoryCountLimit)
				.Where(x => x.IsLong())
				.Select(x => int.Parse(x))
				.Where(x => x > 0)
				.Subscribe(x => SettingsService.Current.HistoryCountLimit = x);

			this.WhenAnyValue(x => x.DetailWindowTimeoutLength)
				.Where(x => x.IsLong())
				.Select(x => int.Parse(x))
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

		public string HFName
		{
			get { return _hfName; }
			set { this.RaiseAndSetIfChanged(ref _hfName, value); }
		}

		public object SelectedRemoteControl2012HF
		{
			get { return _selectedRemoteControl2012HF; }
			set { this.RaiseAndSetIfChanged(ref _selectedRemoteControl2012HF, value); }
		}



		public Task Opening(Action close, object parameter)
		{
			_close = close;
			return Task.FromResult<object>(null);
		}
	}
}
