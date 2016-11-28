using Akavache;
using ReactiveUI;
using Splat;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;

namespace SupportTool.Services.SettingsServices
{
	public partial class SettingsService : IEnableLogger
	{
		public static SettingsService Current { get; private set; }
		static SettingsService() { Current = Current ?? new SettingsService(); }



		private SettingsService()
		{
			this.Log().Info("Loading settings");
		}

		public static void Init() => BlobCache.ApplicationName = "Magnum Opus";

		public static void Shutdown() => BlobCache.Shutdown().Wait();



		private T Get<T>(T defaultValue = default(T), [CallerMemberName]string key = null) => BlobCache.LocalMachine.GetObject<T>(key).Catch(Observable.Return(defaultValue)).Wait();

		private async void Set<T>(T value, [CallerMemberName]string key = null) => await BlobCache.LocalMachine.InsertObject(key, value);
	}
}
