using Splat;
using System;
using System.Linq;
using System.ComponentModel;
using System.Reactive.Subjects;
using System.IO;
using System.Security.Principal;
using System.Reactive.Linq;
using MagnumOpus.Settings;

namespace MagnumOpus.Logging
{
	public class FileLoggerService : ILogger
	{
		private const string ApplicationName = "Magnum Opus";
		private static string LogDirectoryPath = Locator.Current.GetService<SettingsFacade>().LogDirectoryPath;
		private readonly string _logFileName;
		private static Subject<string> _logWriter;

		private string GetLogFilePath() => Path.Combine(LogDirectoryPath, _logFileName);



		public FileLoggerService()
		{
			if (!Directory.Exists(LogDirectoryPath)) Directory.CreateDirectory(LogDirectoryPath);

			var now = DateTimeOffset.Now;
			_logFileName = $"{WindowsIdentity.GetCurrent().Name.Split('\\').Last()}.txt";

			_logWriter = new Subject<string>();
			_logWriter
				.Buffer(TimeSpan.FromSeconds(2), 20)
				.Subscribe(message => File.AppendAllLines(GetLogFilePath(), message));
		}



		private static string CreteLogString(string message, LogLevel logLevel) => $"{DateTimeOffset.Now.ToString()}|{logLevel.ToString()}|{message}\n";



		public LogLevel Level
		{
			get { return LogLevel.Fatal; }
			set { /* Do nothing */ }
		}

		public void Write([Localizable(false)] string message, LogLevel logLevel) => _logWriter.OnNext(CreteLogString(message, logLevel));
	}
}
