namespace SupportTool.Services.SettingsServices
{
	public partial class SettingsService
	{
		public int HistoryCountLimit
		{
			get => Get(50);
			set => Set(value);
		}

		public int DetailsWindowTimeoutLength
		{
			get => Get(2);
			set => Set(value); 
		}

        public bool UseEscapeToCloseDetailsWindows
        {
            get => Get(true);
            set => Set(value);
        }

		public string RemoteControlClassicPath
		{
			get => Get(@"C:\SCCM Remote Control\rc.exe");
			set => Set(value);
		}

		public string RemoteControl2012Path
		{
			get => Get(@"C:\RemoteControl2012\CmRcViewer.exe");
			set => Set(value); 
		}
	}
}
