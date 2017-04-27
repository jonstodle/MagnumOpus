using MagnumOpus.Models;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace MagnumOpus.Services.ActiveDirectoryServices
{
	public partial class ActiveDirectoryService
    {
        public IObservable<GroupObject> GetGroup(string identity, IScheduler scheduler = null) => Observable.Start(() =>
        {
            var up = GroupPrincipal.FindByIdentity(_principalContext, identity);
            return up != null ? new GroupObject(up) : null;
        }, scheduler ?? TaskPoolScheduler.Default);

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

		public IObservable<DirectoryEntry> GetParents(IEnumerable<string> initialElements) => new ParentGroupsState(initialElements).Results;

		public IObservable<DirectoryEntry> GetParents(params string[] initialElements) => GetParents(initialElements);

        public string GetNameFromPath(string path) => path.Split(',')[0].Split('=')[1];

        string EscapeString(string str) => str
            .Replace(@"*", @"\2a")
            .Replace(@"(", @"\28")
            .Replace(@")", @"\29")
            .Replace(@"\", @"\5c")
            .Replace(@"NUL", @"\00")
            .Replace(@"/", @"\2f");
    }

	public class ParentGroupsState
	{
		public ParentGroupsState(IEnumerable<string> initialElements)
		{
			Observable.Start(() =>
			{
				foreach (var element in initialElements) GetAllGroups(element);
			})
			.Subscribe()
			.AddTo(_disposables);

			_resultsSubject
				.Subscribe(_ => { },
				() => _disposables.Dispose());
		}



		public IObservable<DirectoryEntry> Results => _resultsSubject;



		private void GetAllGroups(string name)
		{
			var group = ActiveDirectoryService.Current.GetGroup(name).Wait().Principal;
			if (group == null) return;

			var groupName = group.Name;
			var groups = group.GetGroups();

			if (!_history.Contains(groupName))
			{
				_history.Add(groupName);
				_resultsSubject.OnNext(group.GetUnderlyingObject() as DirectoryEntry);

				try { foreach (var memberGroup in groups) GetAllGroups(memberGroup.Name); }
				catch { }
			}


		}



		private CompositeDisposable _disposables = new CompositeDisposable();
		private List<string> _history = new List<string>();
		private ReplaySubject<DirectoryEntry> _resultsSubject = new ReplaySubject<DirectoryEntry>();
	}
}
