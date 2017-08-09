using System.Text.RegularExpressions;

namespace System
{
	public static class SystemMixins
	{
		public static string ToNorwegianString(this DayOfWeek dow)
		{
			var result = "";

			switch (dow)
			{
				case DayOfWeek.Sunday:
					result = "Sondag";
					break;
				case DayOfWeek.Monday:
					result = "Mandag";
					break;
				case DayOfWeek.Tuesday:
					result = "Tirsdag";
					break;
				case DayOfWeek.Wednesday:
					result = "Onsdag";
					break;
				case DayOfWeek.Thursday:
					result = "Torsdag";
					break;
				case DayOfWeek.Friday:
					result = "Fredag";
					break;
				case DayOfWeek.Saturday:
					result = "Lordag";
					break;
				default:
					break;
			}

			return result;
		}

        public static bool HasValue(this string s, int minLength = 1) => !string.IsNullOrWhiteSpace(s) && s.Length >= minLength;

		public static bool IsIPAddress(this string s) => Regex.IsMatch(s, @"^(?:[0-9]{1,3}\.){3}[0-9]{1,3}$");

		public static bool IsInt(this string s) { int tempInt; return int.TryParse(s, out tempInt); }

		public static bool IsLong(this string s) { long tempLong; return long.TryParse(s, out tempLong); }
	}
}
