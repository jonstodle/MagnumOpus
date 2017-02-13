using MagnumOpus.Services.FileServices;
using System;
using System.IO;
using System.Windows;

namespace MagnumOpus.Executables
{
	public static class Helpers
	{
		public static void WriteExecutableToDisk(string executable)
		{
			var rStream = Application.GetResourceStream(new Uri($"pack://application:,,,/Executables/Files/{executable}"));
			using (var fs = new FileStream(Path.Combine(FileService.LocalAppData, executable), FileMode.Create, FileAccess.Write))
			using (var stream = new MemoryStream())
			using (var writer = new BinaryWriter(fs))
			{
				rStream.Stream.CopyTo(stream);
				writer.Write(stream.ToArray());
			}
		}

		public static void EnsureExecutableIsAvailable(string executable)
		{
			if (!File.Exists(Path.Combine(FileService.LocalAppData, executable)))
			{
				WriteExecutableToDisk(executable);
			}
		}
	}
}
