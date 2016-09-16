using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.DirectoryServices
{
    public static class SystemDirectoryServicesExtensions
    {
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
	}
}
