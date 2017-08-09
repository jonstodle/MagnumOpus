using System.Linq;
using System.Reactive.Concurrency;
using MagnumOpus.ActiveDirectory;

namespace System.DirectoryServices.AccountManagement
{
	public static class AccountManagementMixins
	{
		public static IObservable<DirectoryEntry> GetAllGroups(this Principal source, IScheduler scheduler = null) => ActiveDirectoryService.Current.GetParents(source.GetGroups().Select(principal => principal.Name), scheduler);
	}
}
