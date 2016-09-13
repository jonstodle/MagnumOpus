using SupportTool.Models;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupportTool.Services.ActiveDirectoryServices
{
    public partial class ActiveDirectoryService
    {
        public static ActiveDirectoryService Current { get; } = new ActiveDirectoryService();



        PrincipalContext principalContext;

        private ActiveDirectoryService()
        {
            principalContext = new PrincipalContext(ContextType.Domain);
        }



        public IObservable<Principal> GetPrincipal(string identity) => Observable.Start(() => Principal.FindByIdentity(principalContext, identity));

		public IObservable<DirectoryEntry> SearchDirectory(string searchTerm) => Observable.Create<DirectoryEntry>(observer =>
		{
			var disposed = false;

			using (var directoryEntry = new DirectoryEntry("LDAP://sikt.sykehuspartner.no"))
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
    }
}
