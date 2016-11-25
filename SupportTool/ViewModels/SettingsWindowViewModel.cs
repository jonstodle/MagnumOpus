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
			get => SettingsService.Current.HistoryCountLimit.ToString();
			set { if(int.TryParse(value, out int i)) SettingsService.Current.HistoryCountLimit = i; }
		}

		public string DetailWindowTimeoutLength
		{
			get => SettingsService.Current.DetailsWindowTimeoutLength.ToString();
			set { if (int.TryParse(value, out int i)) SettingsService.Current.DetailsWindowTimeoutLength = i; }
		}

		public string RemoteControlClassicPath
		{
			get => SettingsService.Current.RemoteControlClassicPath;
			set => SettingsService.Current.RemoteControlClassicPath = value;
		}

		public string RemoteControl2012Path
		{
			get => SettingsService.Current.RemoteControl2012Path;
			set => SettingsService.Current.RemoteControl2012Path = value;
		}



		public Task Opening(Action close, object parameter)
		{
			_close = close;
			return Task.FromResult<object>(null);
		}
	}
}
