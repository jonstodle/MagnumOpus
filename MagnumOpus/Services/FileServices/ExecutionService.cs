using System;
using System.Diagnostics;
using System.IO;

using static MagnumOpus.Executables.Helpers;

namespace MagnumOpus.Services.FileServices
{
	public static class ExecutionService
	{
		public static string System32Path => Environment.GetFolderPath(Environment.SpecialFolder.System);

		public static void ExecuteCmd(string fileName, string arguments = "", bool showWindow = true) => ExecuteFile(Path.Combine(System32Path, "cmd.exe"), $@"/K {fileName} {arguments}", showWindow);

		public static void ExecuteFile(string fileName, string arguments = "", bool showWindow = true)
		{
			if (File.Exists(fileName)) Process.Start(new ProcessStartInfo(fileName, arguments) { CreateNoWindow = !showWindow });
			else throw new ArgumentException($"Could not find {fileName}");
		}

		public static void ExecuteInternalCmd(string fileName, string arguments = "", bool showWindow = true) => ExecuteFile(Path.Combine(System32Path, "cmd.exe"), $"/K \"{Path.Combine(FileService.LocalAppData, fileName)}\" {arguments}", showWindow);

		public static void ExecuteInternalFile(string fileName, string arguments = "", bool showWindow = true)
		{
			var filePath = Path.Combine(FileService.LocalAppData, fileName);
			EnsureExecutableIsAvailable(fileName);
			ExecuteFile(filePath, arguments, showWindow);
		}
	}
}
