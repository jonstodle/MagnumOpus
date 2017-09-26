using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using MagnumOpus.Group;
using Splat;

namespace MagnumOpus.ActiveDirectory
{
	public partial class ADFacade
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

		public IObservable<DirectoryEntry> GetParents(IEnumerable<string> initialElements, IScheduler scheduler = null) => new ParentGroupsState(initialElements, scheduler).Results;

        public string GetNameFromPath(string path) => path.Split(',')[0].Split('=')[1];
    }

	public class ParentGroupsState
	{
		public ParentGroupsState(IEnumerable<string> initialElements, IScheduler scheduler = null)
		{
			Observable.Start(() =>
			{
				foreach (var element in initialElements) GetAllGroups(element);
			}, scheduler ?? TaskPoolScheduler.Default)
			.Subscribe()
			.AddTo(_disposables);

			_resultsSubject
				.Subscribe(_ => { },
				() => _disposables.Dispose());
		}



		public IObservable<DirectoryEntry> Results => _resultsSubject;



		private void GetAllGroups(string name)
		{
			var group = Locator.Current.GetService<ADFacade>().GetGroup(name).Wait().Principal;
			if (group == null) return;

			var groupName = group.Name;
			if (_history.Contains(groupName)) return;
			
			_history.Add(groupName);
			_resultsSubject.OnNext(group.GetUnderlyingObject() as DirectoryEntry);

			try { foreach (var memberGroup in group.GetGroups()) GetAllGroups(memberGroup.Name); }
			catch { /* Ignored */ }
		}



		private readonly CompositeDisposable _disposables = new CompositeDisposable();
		private readonly List<string> _history = new List<string>();
		private readonly ReplaySubject<DirectoryEntry> _resultsSubject = new ReplaySubject<DirectoryEntry>();
	}
}
