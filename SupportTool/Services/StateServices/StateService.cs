using Akavache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupportTool.Services.StateServices
{
	public class StateService
	{
		public static IObservable<T> Get<T>(string key, T defaultValue = default(T)) => BlobCache.LocalMachine.GetObject<T>(key).Catch(Observable.Return(defaultValue));

		public async static void Set<T>(string key, T value) => await BlobCache.LocalMachine.InsertObject(key, value);
	}
}
