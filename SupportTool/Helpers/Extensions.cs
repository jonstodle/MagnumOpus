using ReactiveUI;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public static bool HasValue(this string s) => !string.IsNullOrWhiteSpace(s);
    }
}
