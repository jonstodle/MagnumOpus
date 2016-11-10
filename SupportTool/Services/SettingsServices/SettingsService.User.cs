using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupportTool.Services.SettingsServices
{
	public partial class SettingsService
	{
		public int HistoryCountLimit
		{
			get { return Get(nameof(HistoryCountLimit), 50); }
			set { Set(nameof(HistoryCountLimit), value); }
		}

		public int DetailsWindowTimeoutLength
		{
			get { return Get(nameof(DetailsWindowTimeoutLength), 2); }
			set { Set(nameof(DetailsWindowTimeoutLength), value); }
		}
	}
}
