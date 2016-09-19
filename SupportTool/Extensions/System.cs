using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace System
{
	public static class SystemExtensions
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

		public static IObservable<T> WhereNotNull<T>(this IObservable<T> source) where T : class => source.Where(x => x != null);

		public static IObservable<bool> IsTrue<T>(this IObservable<bool> source) where T : class => source.Select(x => x == true);

		public static IObservable<bool> IsFalse<T>(this IObservable<bool> source) where T : class => source.Select(x => x == false);

		public static IObservable<Unit> ToSignal<T>(this IObservable<T> source) where T : class => source.Select(_ => Unit.Default);
	}
}
