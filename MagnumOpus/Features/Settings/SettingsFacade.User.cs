namespace MagnumOpus.Settings
{
	public partial class SettingsFacade
	{
		public int HistoryCountLimit
		{
			get => Get(50);
			set => Set(value);
		}

        public bool OpenDuplicateWindows
        {
            get => Get(false);
            set => Set(value);
        }

        public double DetailsWindowTimeoutLength
		{
			get => Get(2d);
			set => Set(value); 
		}

        public bool UseEscapeToCloseDetailsWindows
        {
            get => Get(true);
            set => Set(value);
        }
	}
}
