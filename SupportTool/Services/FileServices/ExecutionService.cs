﻿using System;
using System.Diagnostics;
using System.IO;

using static SupportTool.Executables.Helpers;

namespace SupportTool.Services.FileServices
{
	public static class ExecutionService
	{
		public static string System32Path => Environment.GetFolderPath(Environment.SpecialFolder.System);

		public static void ExecuteCmd(string fileName, string arguments = "") => ExecuteFile(Path.Combine(System32Path, "cmd.exe"), $@"/K {fileName} {arguments}");

		public static void ExecuteFile(string fileName, string arguments = "")
		{
			if (File.Exists(fileName)) Process.Start(fileName, arguments);
			else throw new ArgumentException($"Could not find {fileName}");
		}

		public static void ExecuteInternalFile(string fileName, string arguments = "")
		{
			var filePath = Path.Combine(FileService.LocalAppData, fileName);
			EnsureExecutableIsAvailable(fileName);
			ExecuteFile(filePath, arguments);
		}
	}
}
