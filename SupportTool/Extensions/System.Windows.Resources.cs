using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Windows.Resources
{
	public static class SystemWindowsResourcesExtensions
	{
		public static void WriteToDisk(this StreamResourceInfo sri, string file)
		{
			using (var fs = new FileStream(file, FileMode.Create, FileAccess.Write))
			using (var stream = new MemoryStream())
			using (var writer = new BinaryWriter(fs))
			{
				sri.Stream.CopyTo(stream);
				writer.Write(stream.ToArray());
			}
		}
	}
}
