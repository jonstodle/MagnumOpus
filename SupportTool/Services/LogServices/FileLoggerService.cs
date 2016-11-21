using Splat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Reactive.Subjects;
using System.IO;
using System.Security.Principal;

namespace SupportTool.Services.LogServices
{
	public class FileLoggerService : ILogger
	{
		private const string ApplicationName = "Magnum Opus";
		private const string LogDirectoryPath = @"C:\Logs\Magnum Opus";
		private readonly string _logFileName;
		private static Subject<string> _logWriter;

		private string GetLogFilePath() => Path.Combine(LogDirectoryPath, _logFileName);



		public FileLoggerService()
		{
			if (!Directory.Exists(LogDirectoryPath)) Directory.CreateDirectory(LogDirectoryPath);

			var now = DateTimeOffset.Now;
			_logFileName = $"{WindowsIdentity.GetCurrent().Name.Split('\\').Last()}-{$"{now.Year}{now.Month.ToString("00")}{now.Day.ToString("00")}-{now.Hour.ToString("00")}{now.Minute.ToString("00")}{now.Second.ToString("00")}"}.txt";

			_logWriter = new Subject<string>();
			_logWriter
				.Subscribe(message => File.AppendAllText(GetLogFilePath(), message));
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
