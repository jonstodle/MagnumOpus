using System.Linq;
using MagnumOpus.Services.ActiveDirectoryServices;

namespace System.DirectoryServices.AccountManagement
{
	public static class SystemDirectoryServicesAccountManagement
	{
		public static IObservable<DirectoryEntry> GetAllGroups(this Principal source) => ActiveDirectoryService.Current.GetParents(source.GetGroups().Select(x => x.Name));
	}
}
