using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace SupportTool.Services.LogServices
{
	public static class LogService
	{
		private const string ApplicationName = "Magnum Opus";
		private const string LogDirectoryPath = @"C:\Logs";
		private static readonly string _logFileName;
		private static Subject<string> _logger;

		private static string GetLogFilePath() => Path.Combine(LogDirectoryPath, _logFileName);

		static LogService()
		{
			if (!Directory.Exists(LogDirectoryPath)) Directory.CreateDirectory(LogDirectoryPath);

			_logFileName = $"{WindowsIdentity.GetCurrent().Name.Split('\\').Last()}-{DateTimeOffset.Now.ToFileTime().ToString()}.txt";

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
