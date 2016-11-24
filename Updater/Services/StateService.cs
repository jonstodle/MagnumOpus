using Akavache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Updater.Services
{
	public class StateService
	{
		public static StateService Current { get; } = new StateService();

		private StateService()
		{
			BlobCache.ApplicationName = "Magnum Opus Updater";
		}



		public string SourceFilePath
		{
			get => BlobCache.LocalMachine.GetObject<string>(nameof(SourceFilePath)).Wait();
			set => BlobCache.LocalMachine.InsertObject(nameof(SourceFilePath), value).Wait();
		}

		public IEnumerable<string> DestinationFolders
		{
			get => BlobCache.LocalMachine.GetObject<IEnumerable<string>>(nameof(DestinationFolders)).Wait();
			set => BlobCache.LocalMachine.InsertObject(nameof(DestinationFolders), value).Wait();
		}
	}
}
