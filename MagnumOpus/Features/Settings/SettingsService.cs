using Akavache;
using Newtonsoft.Json;
using Splat;
using System.IO;
using System.Reactive.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace MagnumOpus.Settings
{
	public partial class SettingsService : IEnableLogger
	{
        private static SettingsService _current;
		public static SettingsService Current { get => _current != null ? _current : _current = new SettingsService(); }



		private SettingsService()
		{
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("MagnumOpus.EnvironmentDefaults.json"))
            using (var reader = new StreamReader(stream ?? throw new FileNotFoundException("Could not find the file 'EnvironmentDefaults.json' in the assembly.\nRemember to add a file named 'EnvironmentDefaults.json' in the root of the project and set its build action to 'Embedded Resource'.")))
            {
                _defaults = JsonConvert.DeserializeObject<EnvironmentDefaults>(reader.ReadToEnd());
            }

            this.Log().Info("Loaded settings");
        }

        public static void Init() => BlobCache.ApplicationName = "Magnum Opus";

		public static void Shutdown() => BlobCache.Shutdown().Wait();



		private T Get<T>(T defaultValue = default(T), [CallerMemberName]string key = null) => BlobCache.UserAccount.GetObject<T>(key).CatchAndReturn(defaultValue).Wait();

		private async void Set<T>(T value, [CallerMemberName]string key = null) => await BlobCache.UserAccount.InsertObject(key, value);
	}
}
