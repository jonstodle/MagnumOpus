using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupportTool.Services.ActiveDirectoryServices
{
    public partial class ActiveDirectoryService
    {
        private DirectoryEntry directoryEntry = new DirectoryEntry("LDAP://sikt.sykehuspartner.no");

        public Task<IEnumerable<SearchResult>> GetGroupsForUser(string searchTerm, params string[] propertiesToLoad) => GetGroups("user", "samaccountname", searchTerm, propertiesToLoad);

        public Task<IEnumerable<SearchResult>> GetGroups(string objectCategory, string searchProperty, string searchTerm, params string[] propertiesToLoad)
        {
            return Task.Run<IEnumerable<SearchResult>>(() =>
            {
                using (var searcher = new DirectorySearcher(directoryEntry))
                {
                    searcher.PageSize = 1000;
                    searcher.Filter = $"(&(objectCategory={objectCategory})({searchProperty}={searchTerm}))";

                    foreach (var prop in propertiesToLoad) { searcher.PropertiesToLoad.Add(prop); }

                    var returnValue = new List<SearchResult>();
                    using (var results = searcher.FindAll())
                    {
                        foreach (var result in results) { returnValue.Add((SearchResult)result); }
                    }

                    return returnValue;
                }
            });
        }

        public async Task<IEnumerable<DirectoryEntry>> GetParents(string name, string path, IEnumerable<DirectoryEntry> collection)
        {
            var result = new List<DirectoryEntry>();

            var group = (await GetGroups("group", "distinguishedname", name)).ElementAt(0).GetDirectoryEntry();
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
