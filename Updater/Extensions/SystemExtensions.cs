namespace System
{
	public static class SystemExtensions
	{
		public static bool HasValue(this string source, int minimumLength = 0) => !string.IsNullOrWhiteSpace(source) && source.Length >= minimumLength;
	}
}
