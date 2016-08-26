using SupportTool.Models;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupportTool.Services.ActiveDirectoryServices
{
    public partial class ActiveDirectoryService
    {
        private DirectoryEntry directoryEntry = new DirectoryEntry("LDAP://sikt.sykehuspartner.no");

        public IObservable<GroupObject> GetGroup(string identity) => Observable.Start(() =>
        {
            var up = GroupPrincipal.FindByIdentity(principalContext, identity);
            return up != null ? new GroupObject(up) : null;
        });

        //public IObservable<DirectoryEntry> GetGroupsForUser(string searchTerm, params string[] propertiesToLoad) => GetGroups("user", "samaccountname", searchTerm, propertiesToLoad);

        public IObservable<DirectoryEntry> GetGroups(string searchProperty, string searchTerm, params string[] propertiesToLoad) => Observable.Create<DirectoryEntry>(observer =>
        {
            var disposed = false;

            using (var searcher = new DirectorySearcher(directoryEntry, $"(&(objectCategory=group)({searchProperty}={searchTerm}))", propertiesToLoad))
            {
                searcher.PageSize = 1000;

                using (var results = searcher.FindAll())
                {
                    foreach (SearchResult result in results)
                    {
                        if (disposed) break;
                        observer.OnNext(result.GetDirectoryEntry());
                    }
                }

                observer.OnCompleted();
                return () => disposed = true;
            }
        });

        public async Task<IEnumerable<DirectoryEntry>> GetParents(string name, string path, IEnumerable<DirectoryEntry> collection)
        {
            var result = new List<DirectoryEntry>();

            var group = await GetGroups("distinguishedname", name).Take(1);
            result.Add(group);
            var memberof = group.Properties["memberof"];

            foreach (var element in memberof)
            {
                var memberofName = element.ToString();
                if (collection.Any(x => x.Properties["distinguishedname"][0].ToString() == element.ToString())) continue;
                else result.AddRange(await GetParents(memberofName, path + "/" + memberofName, result));
            }

            return result.Distinct(new DirectoryEntryComparer());
        }

        public string GetNameFromPath(string path) => path.Split(',')[0].Split('=')[1];

        string EscapeString(string str) => str
            .Replace(@"*", @"\2a")
            .Replace(@"(", @"\28")
            .Replace(@")", @"\29")
            .Replace(@"\", @"\5c")
            .Replace(@"NUL", @"\00")
            .Replace(@"/", @"\2f");
    }

    class DirectoryEntryComparer : IEqualityComparer<DirectoryEntry>
    {
        public bool Equals(DirectoryEntry x, DirectoryEntry y) => x.Path == y.Path;

        public int GetHashCode(DirectoryEntry obj) => obj.Path.Select(x => (int)x).Aggregate((prev, curr) => prev + curr);
    }
}
