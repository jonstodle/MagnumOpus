using System.Linq;
using MagnumOpus.Services.ActiveDirectoryServices;
using System.Reactive.Concurrency;

namespace System.DirectoryServices.AccountManagement
{
	public static class SystemDirectoryServicesAccountManagement
	{
		public static IObservable<DirectoryEntry> GetAllGroups(this Principal source, IScheduler scheduler = null) => ActiveDirectoryService.Current.GetParents(source.GetGroups().Select(x => x.Name), scheduler);
	}
}
