using System;
using System.Reflection;
using System.Threading.Tasks;
using MagnumOpus.Dialog;
using Splat;

namespace MagnumOpus.Settings
{
	public class SettingsWindowViewModel : ViewModelBase, IDialog
	{
		public SettingsWindowViewModel() {
            Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }



		public string HistoryCountLimit
		{
			get => _settings.HistoryCountLimit.ToString();
			set { if(int.TryParse(value, out int i)) _settings.HistoryCountLimit = i; }
        }

        public bool OpenDuplicateWindows
        {
            get => _settings.OpenDuplicateWindows;
            set => _settings.OpenDuplicateWindows = value;
        }

        public string DetailWindowTimeoutLength
		{
			get => _settings.DetailsWindowTimeoutLength.ToString();
			set { if (double.TryParse(value, out double i)) _settings.DetailsWindowTimeoutLength = i > 0 ? i : 1; }
		}

        public bool UseEscapeToCloseDetailsWindows
        {
            get => _settings.UseEscapeToCloseDetailsWindows;
            set => _settings.UseEscapeToCloseDetailsWindows = value;
        }

        public string Version { get; }


		public Task Opening(Action close, object parameter) => Task.FromResult<object>(null);



		private readonly SettingsFacade _settings = Locator.Current.GetService<SettingsFacade>();
	}
}
