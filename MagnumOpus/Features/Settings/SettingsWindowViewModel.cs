using System;
using System.Reflection;
using System.Threading.Tasks;
using MagnumOpus.Dialog;

namespace MagnumOpus.Settings
{
	public class SettingsWindowViewModel : ViewModelBase, IDialog
	{
		public SettingsWindowViewModel() {
            Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
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
