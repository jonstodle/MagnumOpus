using MagnumOpus.Services.SettingsServices;
using System;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Security.Principal;

namespace MagnumOpus.Services.LogServices
{
	public static class LogService
	{
		private const string ApplicationName = "Magnum Opus";
        private static string LogDirectoryPath = SettingsService.Current.LogDirectoryPath;
		private static readonly string _logFileName;
		private static Subject<string> _logger;

		private static string GetLogFilePath() => Path.Combine(LogDirectoryPath, _logFileName);

		static LogService()
		{
			if (!Directory.Exists(LogDirectoryPath)) Directory.CreateDirectory(LogDirectoryPath);

			var now = DateTimeOffset.Now;
			_logFileName = $"{WindowsIdentity.GetCurrent().Name.Split('\\').Last()}-{$"{now.Year}{now.Month.ToString("00")}{now.Day.ToString("00")}-{now.Hour.ToString("00")}{now.Minute.ToString("00")}{now.Second.ToString("00")}"}.txt";

			_logger = new Subject<string>();
			_logger
				.Subscribe(message => File.AppendAllText(GetLogFilePath(), message));
		}

		private static string CreteLogString(string message, LogType type) => $"{DateTimeOffset.Now.ToString()}|{type.ToString()}|{message}\n";



		public static void Write(string message, LogType type) => _logger.OnNext(CreteLogString(message, type));

		public static void Info(string message) => Write(message, LogType.Information);

		public static void Warning(string message) => Write(message, LogType.Warning);

		public static void Error(string message) => Write(message, LogType.Error);
	}
}
