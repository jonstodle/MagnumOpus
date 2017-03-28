using MagnumOpus.Services.NavigationServices;
using MagnumOpus.Services.SettingsServices;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace MagnumOpus.ViewModels
{
	public class SettingsWindowViewModel : ViewModelBase, IDialog
	{
		public SettingsWindowViewModel() {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            var assemblyTime = Assembly.GetExecutingAssembly().GetLinkerTime();
            _version = $"{version.Major}.{version.Minor}.{assemblyTime.Day.ToString("00")}{assemblyTime.Month.ToString("00")}{assemblyTime.Year.ToString().Substring(2, 2)}.{assemblyTime.Hour.ToString("00")}{assemblyTime.Minute.ToString("00")}{assemblyTime.Second.ToString("00")}";
        }



		public string HistoryCountLimit
		{
			get => SettingsService.Current.HistoryCountLimit.ToString();
			set { if(int.TryParse(value, out int i)) SettingsService.Current.HistoryCountLimit = i; }
        }

        public bool OpenDuplicateWindows
        {
            get => SettingsService.Current.OpenDuplicateWindows;
            set => SettingsService.Current.OpenDuplicateWindows = value;
        }

        public string DetailWindowTimeoutLength
		{
			get => SettingsService.Current.DetailsWindowTimeoutLength.ToString();
			set { if (double.TryParse(value, out double i)) SettingsService.Current.DetailsWindowTimeoutLength = i > 0 ? i : 1; }
		}

        public bool UseEscapeToCloseDetailsWindows
        {
            get => SettingsService.Current.UseEscapeToCloseDetailsWindows;
            set => SettingsService.Current.UseEscapeToCloseDetailsWindows = value;
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

        public string Version => _version;



		public Task Opening(Action close, object parameter)
		{
			_close = close;
			return Task.FromResult<object>(null);
		}



        private readonly string _version;
        private Action _close;
    }
}
