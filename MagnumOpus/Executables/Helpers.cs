using System;
using System.IO;
using System.IO.Compression;
using System.Windows;
using MagnumOpus.FileHelpers;

namespace MagnumOpus.Executables
{
	public static class Helpers
	{
        /// <summary>
        /// Writes the given application's file(s) to %LOCALAPPDATA%\Magnum Opus by copying the zip file from \Executables\Files and extracting it
        /// </summary>
        /// <param name="applicationName">The name of the application</param>
		public static void WriteApplicationFilesToDisk(string applicationName)
		{
            var applicationDirectoryPath = Path.Combine(FileService.LocalAppData, applicationName);
            if (Directory.Exists(applicationDirectoryPath)) return;

            var fileName = $"{applicationName}.zip";
            var fullFilePath = Path.Combine(FileService.LocalAppData, fileName);

            var resourceStream = Application.GetResourceStream(new Uri($"pack://application:,,,/Executables/Files/{fileName}"));
			using (var fileStream = new FileStream(fullFilePath, FileMode.Create, FileAccess.Write))
			using (var memoryStream = new MemoryStream())
			using (var writer = new BinaryWriter(fileStream))
			{
				resourceStream.Stream.CopyTo(memoryStream);
				writer.Write(memoryStream.ToArray());
			}

            Directory.CreateDirectory(applicationDirectoryPath);
            ZipFile.ExtractToDirectory(fullFilePath, applicationDirectoryPath);
            File.Delete(fullFilePath);
		}
	}
}
