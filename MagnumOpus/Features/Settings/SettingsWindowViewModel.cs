using System;
using System.Reflection;
using System.Threading.Tasks;
using MagnumOpus.Dialog;

namespace MagnumOpus.Settings
{
	public class SettingsWindowViewModel : ViewModelBase, IDialog
	{
		public SettingsWindowViewModel() {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            var assemblyTime = Assembly.GetExecutingAssembly().GetLinkerTime();
            Version = $"{version.Major}.{version.Minor}.{assemblyTime.Day:00}{assemblyTime.Month:00}{assemblyTime.Year.ToString().Substring(2, 2)}.{assemblyTime.Hour:00}{assemblyTime.Minute:00}{assemblyTime.Second:00}";
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

        public string Version { get; }


		public Task Opening(Action close, object parameter) => Task.FromResult<object>(null);
	}
}
