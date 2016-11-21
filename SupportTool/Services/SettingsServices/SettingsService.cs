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
using System.Text;
using System.Threading.Tasks;

namespace SupportTool.Services.SettingsServices
{
	public partial class SettingsService : IEnableLogger
	{
		public static SettingsService Current { get; private set; }



		private const string SettingsIdentifier = "settings";
		private Dictionary<string, object> _settings;
		private readonly ReactiveCommand<Unit, IObservable<Unit>> _saveSettings;

		private SettingsService()
		{
			this.Log().Info("Loading settings");

			_settings = FileService.DeserializeFromDisk<Dictionary<string, object>>(SettingsIdentifier)
				.Catch(Observable.Return(new Dictionary<string, object>()))
				.Wait();

			_saveSettings = ReactiveCommand.Create(() => FileService.SerializeToDisk(SettingsIdentifier, _settings));
			_saveSettings
				.Throttle(TimeSpan.FromSeconds(2))
				.Switch()
				.Subscribe(_ => this.Log().Info("Saved settings"));
		}

		public static void Init() => Current = new SettingsService();



		private T Get<T>(string key)
		{
			if (_settings.ContainsKey(key)) return JsonConvert.DeserializeObject<T>(_settings[key].ToString());
			else return default(T);
		}

		private T Get<T>(string key, T defaultValue)
		{
			if (_settings.ContainsKey(key)) return Get<T>(key);
			else return defaultValue;
		}

		private async void Set<T>(string key, T value)
		{
			_settings[key] = value;
			await _saveSettings.Execute();
		}
	}
}
