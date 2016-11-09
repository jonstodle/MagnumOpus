using System;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices.ActiveDirectory;
using System.Net.NetworkInformation;
using System.Reactive.Linq;

namespace SupportTool.Services.ActiveDirectoryServices
{
	public partial class ActiveDirectoryService
    {
        public static ActiveDirectoryService Current { get; } = new ActiveDirectoryService();



        private readonly PrincipalContext _principalContext = new PrincipalContext(ContextType.Domain);
		private readonly string _currentDomain = Domain.GetCurrentDomain().Name;


		private ActiveDirectoryService() { }



        public IObservable<Principal> GetPrincipal(string identity) => Observable.Start(() => Principal.FindByIdentity(_principalContext, identity));

		public IObservable<DirectoryEntry> SearchDirectory(string searchTerm) => Observable.Create<DirectoryEntry>(observer =>
		{
			var disposed = false;

			using (var directoryEntry = GetDomainDirectoryEntry())
			using (var searcher = new DirectorySearcher(directoryEntry, $"(&(|(objectClass=user)(objectClass=group))(|(userPrincipalName={searchTerm}*)(distinguishedName={searchTerm}*)(name={searchTerm}*)))"))
			{
				foreach (SearchResult result in searcher.FindAll())
				{
					if (disposed) break;
					observer.OnNext(result.GetDirectoryEntry());
				}
			}

			observer.OnCompleted();
			return () => disposed = true;
		});

		private DirectoryEntry GetDomainDirectoryEntry() => new DirectoryEntry($"LDAP://{_currentDomain}");

	}
}
