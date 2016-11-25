using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
	}
}
