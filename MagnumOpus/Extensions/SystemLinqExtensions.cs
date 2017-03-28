using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Linq
{
    public static class SystemLinqExtensions
    {
        public static IEnumerable<T> ToGeneric<T>(this IEnumerable source) { foreach (var item in source) yield return (T)item; }
    }
}
