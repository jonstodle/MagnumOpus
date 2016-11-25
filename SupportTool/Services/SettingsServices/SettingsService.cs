using Akavache;
using Newtonsoft.Json;
using ReactiveUI;
using Splat;
using SupportTool.Services.FileServices;
using SupportTool.Services.LogServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SupportTool.Services.SettingsServices
{
	public partial class SettingsService : IEnableLogger
	{
		public static SettingsService Current { get; private set; }
		static SettingsService() { Current = Current ?? new SettingsService(); }



		private SettingsService()
		{
			this.Log().Info("Loading settings");

			BlobCache.ApplicationName = "Magnum Opus";
		}

		public static void Shutdown() => BlobCache.Shutdown().Wait();



		private T Get<T>(T defaultValue = default(T), [CallerMemberName]string key = null) => BlobCache.LocalMachine.GetObject<T>(key).Catch(Observable.Return(defaultValue)).Wait();

		private async void Set<T>(T value, [CallerMemberName]string key = null) => await BlobCache.LocalMachine.InsertObject(key, value);
	}
}
