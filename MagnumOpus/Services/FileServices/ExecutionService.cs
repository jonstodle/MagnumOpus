using System;
using System.Diagnostics;
using System.IO;

using static MagnumOpus.Executables.Helpers;

namespace MagnumOpus.Services.FileServices
{
	public static class ExecutionService
	{
		public static string System32Path => Environment.GetFolderPath(Environment.SpecialFolder.System);



        public static void RunInCmd(string filePath, string arguments = "", bool showWindow = true) => RunFile(Path.Combine(System32Path, "cmd.exe"), $@"/K {filePath} {arguments}", showWindow);

		public static void RunFile(string filePath, string arguments = "", bool showWindow = true)
		{
			if (File.Exists(filePath)) Process.Start(new ProcessStartInfo(filePath, arguments) { CreateNoWindow = !showWindow });
			else throw new ArgumentException($"Could not find {filePath}");
		}



        public static void RunInCmdFromCache(string fileName, string arguments = "", bool showWindow = true) => RunFile(Path.Combine(System32Path, "cmd.exe"), $"/K \"{Path.Combine(FileService.LocalAppData, fileName)}\" {arguments}", showWindow);

		public static void RunFileFromCache(string fileName, string arguments = "", bool showWindow = true)
		{
			var filePath = Path.Combine(FileService.LocalAppData, fileName);
			EnsureExecutableIsAvailable(fileName);
			RunFile(filePath, arguments, showWindow);
		}
	}
}
