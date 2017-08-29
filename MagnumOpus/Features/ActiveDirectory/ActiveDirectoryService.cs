using System;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices.ActiveDirectory;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace MagnumOpus.ActiveDirectory
{
    public partial class ActiveDirectoryService
    {
        private static ActiveDirectoryService _current;
        public static ActiveDirectoryService Current => _current ?? (_current = new ActiveDirectoryService());

        public static bool IsInDomain()
        {
            try { return Domain.GetComputerDomain()?.Name != null; }
            catch { return false; }
        }



        private readonly PrincipalContext _principalContext = new PrincipalContext(ContextType.Domain);
        public string CurrentDomain => Domain.GetCurrentDomain()?.Name;

        private string _currentDomainShortName;
        public string CurrentDomainShortName
        {
            get
            {
                if (_currentDomainShortName == null)
                {
                    var partitions = new DirectoryEntry(@"LDAP://cn=Partitions," + new DirectoryEntry(@"LDAP://RootDSE").Properties["configurationNamingContext"].Value);
                    using (var searcher = new DirectorySearcher(partitions, "(&(objectcategory=Crossref)(netBIOSName=*))", new[] { "netBIOSName" }))
                    {
                        _currentDomainShortName = searcher.FindOne().Properties["netBIOSName"]?[0]?.ToString();
                    }
                }
                return _currentDomainShortName;
            }
        }


        private ActiveDirectoryService() { }



        public PrincipalType DeterminePrincipalType(string identity) => DeterminePrincipalType(GetPrincipal(identity).Wait());

        public PrincipalType DeterminePrincipalType(Principal principal)
        {
            switch (principal)
            {
                case UserPrincipal _: return PrincipalType.User;
                case ComputerPrincipal _: return PrincipalType.Computer;
                case GroupPrincipal _: return PrincipalType.Group;
                default: return PrincipalType.Generic;
            }
        }

        public IObservable<Principal> GetPrincipal(string identity, IScheduler scheduler = null) => Observable.Start(() => Principal.FindByIdentity(_principalContext, identity), scheduler ?? TaskPoolScheduler.Default);

        public IObservable<DirectoryEntry> SearchDirectory(string searchTerm, IScheduler scheduler = null) => 
            Observable.Create<DirectoryEntry>(observer =>
                (scheduler ?? TaskPoolScheduler.Default).Schedule(() =>
                {
                    if (!searchTerm.EndsWith("*")) searchTerm = searchTerm + "*"; 
                        
                    using (var directoryEntry = GetDomainDirectoryEntry())
                    using (var searcher = new DirectorySearcher(directoryEntry, $"(&(|(objectClass=user)(objectClass=group))(|(userPrincipalName={searchTerm})(distinguishedName={searchTerm})(name={searchTerm})))"))
                    {
                        foreach (SearchResult result in searcher.FindAll())
                        {
                            observer.OnNext(result.GetDirectoryEntry());
                        }
                    }

                    observer.OnCompleted();
                }));

        private DirectoryEntry GetDomainDirectoryEntry() => new DirectoryEntry($"LDAP://{CurrentDomain}");

    }

    public enum PrincipalType
    {
        Generic, User, Computer, Group
    }
}
