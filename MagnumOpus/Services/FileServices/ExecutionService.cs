using System;
using System.Diagnostics;
using System.IO;

using static MagnumOpus.Executables.Helpers;

namespace MagnumOpus.Services.FileServices
{
	public static class ExecutionService
	{
        /// <summary>
        /// Path to the System32 folder on the system
        /// </summary>
		public static string System32Path => Environment.GetFolderPath(Environment.SpecialFolder.System);



        /// <summary>
        /// Runs a file in CMD with the given arguments
        /// </summary>
        /// <param name="filePath">Full path to the file to be executed</param>
        /// <param name="arguments">Arguments to be passed</param>
        /// <param name="showWindow">Set to false to hide CMD window when executing the file</param>
        public static void RunInCmd(string filePath, string arguments = "", bool showWindow = true) => RunFile(Path.Combine(System32Path, "cmd.exe"), $@"/K ""{filePath}"" {arguments}", showWindow);

        /// <summary>
        /// Runs a file in CMD with the given arguments and returns the output
        /// </summary>
        /// <param name="filePath">Full path to the file to be executed</param>
        /// <param name="arguments">Arguments to be passed</param>
        /// <returns>A string containing the output of the executed file</returns>
        public static string RunInCmdWithOuput(string filePath, string arguments = "")
        {
            if (!File.Exists(filePath)) throw new ArgumentException($"Could not find {filePath}");

            return Process.Start(new ProcessStartInfo
            {
                FileName = filePath,
                Arguments = arguments,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            })
            .StandardOutput
            .ReadToEnd();
        }

        /// <summary>
        /// Runs a file in a new process with the given arguments
        /// </summary>
        /// <param name="filePath">Full path to the file to be executed</param>
        /// <param name="arguments">Arguments to be passed</param>
        /// <param name="showWindow">Set to false to hide window</param>
        public static void RunFile(string filePath, string arguments = "", bool showWindow = true)
		{
			if (File.Exists(filePath)) Process.Start(new ProcessStartInfo(filePath, arguments) { CreateNoWindow = !showWindow });
			else throw new ArgumentException($"Could not find {filePath}");
		}



        /// <summary>
        /// Runs a file bundled in the application package in CMD with the given arguments
        /// </summary>
        /// <param name="fileName">Name of the file</param>
        /// <param name="arguments">Arguments to be passed</param>
        /// <param name="showWindow">Set to false to hide window</param>
        public static void RunInCmdFromCache(string folderName, string fileName, string arguments = "", bool showWindow = true)
        {
            WriteApplicationFilesToDisk(folderName);
            RunFile(Path.Combine(System32Path, "cmd.exe"), $@"/K ""{Path.Combine(FileService.LocalAppData, folderName, fileName)}"" {arguments}", showWindow);
        }

        /// <summary>
        /// Runs a file bundled in the application package in a new process with the given arguments
        /// </summary>
        /// <param name="fileName">Name of the file</param>
        /// <param name="arguments">Arguments to be passed</param>
        /// <param name="showWindow">Set to false to hide window</param>
		public static void RunFileFromCache(string folderName, string fileName, string arguments = "", bool showWindow = true)
		{
			WriteApplicationFilesToDisk(folderName);
			RunFile(Path.Combine(FileService.LocalAppData, folderName, fileName), arguments, showWindow);
		}
	}
}
