using System.Linq;
using System.Reactive.Concurrency;
using MagnumOpus.ActiveDirectory;
using Splat;

namespace System.DirectoryServices.AccountManagement
{
	public static class AccountManagementMixins
	{
		public static IObservable<DirectoryEntry> GetAllGroups(this Principal source, IScheduler scheduler = null) => Locator.Current.GetService<ADFacade>().GetParents(source.GetGroups().Select(principal => principal.Name), scheduler);
	}
}
