using Akavache;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
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

		public Task Shutdown() => BlobCache.Shutdown();



		public string SourceFilePath
		{
			get => Get(nameof(SourceFilePath), "");
			set => Set(nameof(SourceFilePath), value);
		}

		public IEnumerable<string> DestinationFolders
		{
			get => Get(nameof(DestinationFolders), Enumerable.Empty<string>());
			set => Set(nameof(DestinationFolders), value);
		}

		public bool KillProcesses
		{
			get => Get(nameof(KillProcesses), true);
			set => Set(nameof(KillProcesses), value);
		}



		private T Get<T>(string key, T defaultValue = default(T)) => BlobCache.LocalMachine.GetObject<T>(key).Catch(Observable.Return(defaultValue)).Wait();

		private void Set<T>(string key, T value) => BlobCache.LocalMachine.InsertObject(key, value).Wait();
	}
}
