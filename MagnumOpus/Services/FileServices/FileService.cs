using Newtonsoft.Json;
using System;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;

namespace MagnumOpus.Services.FileServices
{
	public class FileService
	{
        /// <summary>
        /// The path to %LOCALAPPDATA%\Magnum Opus
        /// </summary>
		public static readonly string LocalAppData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Magnum Opus");
        /// <summary>
        /// The path %APPDATA%\Magnum Opus
        /// </summary>
		public static readonly string AppData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Magnum Opus");



        /// <summary>
        /// Writes the data string to %LOCALAPPDATA%\Magnum Opus. This will overwrite any data written previously with the same key
        /// </summary>
        /// <param name="key">An identifier of the data (will also be used as file name)</param>
        /// <param name="data">A string containing the data to be written</param>
        /// <returns></returns>
		public static IObservable<Unit> WriteToLocalAppData(string key, string data) => Observable.Start(() =>
		{
			Directory.CreateDirectory(LocalAppData);
			File.WriteAllText(Path.Combine(LocalAppData, $"{key}.txt"), data);
		});

        /// <summary>
        /// Writes the data string to %APPDATA%\Magnum Opus. This will overwrite any data written previously with the same key
        /// </summary>
        /// <param name="key">An identifier of the data (will also be used as file name)</param>
        /// <param name="data">A string containing the data to be writter</param>
        /// <returns></returns>
		public static IObservable<Unit> WriteToAppData(string key, string data) => Observable.Start(() =>
		{
			Directory.CreateDirectory(AppData);
			File.WriteAllText(Path.Combine(AppData, $"{key}.txt"), data);
		});

        /// <summary>
        /// Reads the string data from %LOCALAPPDATA%\Magnum Opus. Throws an exception if there's no data for the given key
        /// </summary>
        /// <param name="key">The identifier of the data</param>
        /// <returns></returns>
		public static IObservable<string> ReadFromLocalAppData(string key) => Observable.Start(() => File.ReadAllText(Path.Combine(LocalAppData, $"{key}.txt")));

        /// <summary>
        /// Read the string data from %APPDATA%\Magnum Opus. Throws an exception if there's no data for the give key
        /// </summary>
        /// <param name="key">The identifier of the data</param>
        /// <returns></returns>
		public static IObservable<string> ReadFromAppData(string key) => Observable.Start(() => File.ReadAllText(Path.Combine(AppData, $"{key}.txt")));



        /// <summary>
        /// Serializes the data to JSON using Json.NET and writes it to %LOCALAPPDATA%\Magnum Opus
        /// </summary>
        /// <param name="key">An identifier of the data (will also be used as file name)</param>
        /// <param name="data">The data to be stored</param>
        /// <returns></returns>
		public static IObservable<Unit> SerializeToDisk<T>(string key, T data) => WriteToLocalAppData(key, JsonConvert.SerializeObject(data));

        /// <summary>
        /// Reads the data from %LOCALAPPDATA%\Magnum Opus and attempts to deserialize JSON using Json.NET
        /// </summary>
        /// <typeparam name="T">The type to deserialize the data to</typeparam>
        /// <param name="key">The dientifier of the data</param>
        /// <returns></returns>
		public static IObservable<T> DeserializeFromDisk<T>(string key) => Observable.Start(() => JsonConvert.DeserializeObject<T>(ReadFromLocalAppData(key).Wait()));
	}
}
