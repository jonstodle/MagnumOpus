using System.Collections.Generic;

namespace System.DirectoryServices
{
	public static class DirectoryServicesMixins
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

        public static T Get<T>(this ResultPropertyCollection rpc, string propertyName, int index = 0)
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
