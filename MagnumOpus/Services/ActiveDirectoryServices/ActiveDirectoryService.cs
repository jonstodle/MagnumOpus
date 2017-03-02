using System;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices.ActiveDirectory;
using System.Reactive.Linq;

namespace MagnumOpus.Services.ActiveDirectoryServices
{
	public partial class ActiveDirectoryService
    {
        public static ActiveDirectoryService Current { get; } = new ActiveDirectoryService();



        private readonly PrincipalContext _principalContext = new PrincipalContext(ContextType.Domain);
		public string CurrentDomain => Domain.GetCurrentDomain()?.Name;


		private ActiveDirectoryService() { }



        public PrincipalType DeterminePrincipalType(string identity) => DeterminePrincipalType(GetPrincipal(identity).Wait());

		public PrincipalType DeterminePrincipalType(Principal principal)
		{
			if (principal is UserPrincipal) return PrincipalType.User;
			else if (principal is ComputerPrincipal) return PrincipalType.Computer;
			else if (principal is GroupPrincipal) return PrincipalType.Group;
			else return PrincipalType.Generic;
		}

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

		private DirectoryEntry GetDomainDirectoryEntry() => new DirectoryEntry($"LDAP://{CurrentDomain}");

	}

	public enum PrincipalType
	{
		Generic, User, Computer, Group
	}
}
