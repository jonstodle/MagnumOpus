using MagnumOpus.Services.FileServices;
using System;
using System.IO;
using System.Windows;

namespace MagnumOpus.Executables
{
	public static class Helpers
	{
        /// <summary>
        /// Writes the given file to %LOCALAPPDATA%\Magnum Opus by copying it from \Executables\Files
        /// </summary>
        /// <param name="fileName">The name of the file</param>
		public static void WriteFileToDisk(string fileName)
		{
			var rStream = Application.GetResourceStream(new Uri($"pack://application:,,,/Executables/Files/{fileName}"));
			using (var fs = new FileStream(Path.Combine(FileService.LocalAppData, fileName), FileMode.Create, FileAccess.Write))
			using (var stream = new MemoryStream())
			using (var writer = new BinaryWriter(fs))
			{
				rStream.Stream.CopyTo(stream);
				writer.Write(stream.ToArray());
			}
		}

        /// <summary>
        /// Ensures the given file is available in %LOCALAPPDATA%\Magnum Opus. If it's not, it will copy the file from \Executables\Files
        /// </summary>
        /// <param name="fileName">The file name to check for</param>
		public static void EnsureFileIsAvailable(string fileName)
		{
			if (!File.Exists(Path.Combine(FileService.LocalAppData, fileName)))
			{
				WriteFileToDisk(fileName);
			}
		}
	}
}
