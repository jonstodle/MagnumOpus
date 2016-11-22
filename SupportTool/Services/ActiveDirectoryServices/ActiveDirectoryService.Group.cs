using SupportTool.Models;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Reactive.Linq;

namespace SupportTool.Services.ActiveDirectoryServices
{
	public partial class ActiveDirectoryService
    {
        public IObservable<GroupObject> GetGroup(string identity) => Observable.Start(() =>
        {
            var up = GroupPrincipal.FindByIdentity(_principalContext, identity);
            return up != null ? new GroupObject(up) : null;
        });

        public IObservable<DirectoryEntry> GetGroups(string searchProperty, string searchTerm, params string[] propertiesToLoad) => Observable.Create<DirectoryEntry>(observer =>
        {
            var disposed = false;

			using (var directoryEntry = GetDomainDirectoryEntry())
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

        public IObservable<DirectoryEntry> GetParents(string name)
        {
            var group = GetGroups("distinguishedname", EscapeString(name)).Take(1).Wait();
            var memberof = group.Properties["memberof"];

            if (memberof.Count > 0)
                return Observable.Return(group)
                     .Concat(group.Properties["memberof"].ToEnumerable<string>()
                     .ToObservable()
                     .SelectMany(x => GetParents(x))).Distinct(x => x.Path);

            return Observable.Return(group);
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
}
