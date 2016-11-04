using Akavache;
using Newtonsoft.Json;
using ReactiveUI;
using SupportTool.Services.FileServices;
using SupportTool.Services.LogServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupportTool.Services.SettingsServices
{
	public partial class SettingsService
	{
		public static SettingsService Current { get; private set; }



		private const string SettingsIdentifier = "settings";
		private Dictionary<string, object> _settings;
		private readonly ReactiveCommand<Unit, IObservable<Unit>> _saveSettings;

		private SettingsService()
		{
			LogService.Info("Loading settings");

			BlobCache.ApplicationName = "Magnum Opus";
		}

		public static void Init() => Current = new SettingsService();



		private T Get<T>(string key, T defaultValue = default(T))
		{
			return BlobCache.LocalMachine.GetObject<T>(key)
				.Catch(Observable.Return(defaultValue))
				.Wait();
		}

		private void Set<T>(string key, T value)
		{
			BlobCache.LocalMachine.InsertObject(key, value)
				.Subscribe();
		}
	}
}
