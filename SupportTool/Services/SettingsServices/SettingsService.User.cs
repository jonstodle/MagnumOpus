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
		public long HistoryCountLimit
		{
			get { return Get(nameof(HistoryCountLimit), 50L); }
			set { Set(nameof(HistoryCountLimit), value); }
		}

		public long DetailsWindowTimeoutLength
		{
			get { return Get(nameof(DetailsWindowTimeoutLength), 2L); }
			set { Set(nameof(DetailsWindowTimeoutLength), value); }
		}
	}
}
