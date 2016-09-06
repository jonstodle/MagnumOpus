using ReactiveUI;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Resources;

namespace SupportTool.Helpers
{
    public static class Extensions
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

        public static T Get<T>(this PropertyCollection rpc, string propertyName, int index = 0)
        {
            var propCollection = rpc[propertyName];

            if (propCollection.Count == 0
                || index >= propCollection.Count
                || index < 0)
            { return default(T); }

            return (T)propCollection[index];
        }

        public static IEnumerable<T> ToEnumerable<T>(this PropertyValueCollection pvc)
        {
            var result = new List<T>();
            foreach (T val in pvc) result.Add(val);
            return result;
        }

        public static bool HasValue(this string s, int minLength = 1) => !string.IsNullOrWhiteSpace(s) && s.Length >= minLength;

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

		public static IObservable<T> NotNull<T>(this IObservable<T> source) where T : class => source.Where(x => x != null);
    }
}
