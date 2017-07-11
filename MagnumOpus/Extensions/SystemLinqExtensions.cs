using System.Collections;
using System.Collections.Generic;

namespace System.Linq
{
    public static class SystemLinqExtensions
    {
        public static IEnumerable<T> ToGeneric<T>(this IEnumerable source) { foreach (var item in source) yield return (T)item; }
    }
}
