using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
	public static class SystemExtensions
	{
		public static bool HasValue(this string source, int minimumLength = 0) => !string.IsNullOrWhiteSpace(source) && source.Length >= minimumLength;
	}
}
