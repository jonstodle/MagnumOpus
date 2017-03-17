using MagnumOpus.Models;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace MagnumOpus.Services.ActiveDirectoryServices
{
	public partial class ActiveDirectoryService
    {
        public IObservable<UserObject> GetUser(string identity) => Observable.Start(() =>
        {
            var up = UserPrincipal.FindByIdentity(_principalContext, identity);
            return up != null ? new UserObject(up) : null;
        });

		public IObservable<DirectoryEntry> GetUsers(string searchTerm, params string[] propertiesToLoad) => Observable.Create<DirectoryEntry>(observer =>
		{
			var disposed = false;

			using (var directoryEntry = GetDomainDirectoryEntry())
			using (var searcher = new DirectorySearcher(directoryEntry, $"(&(objectCategory=user)({searchTerm}))", propertiesToLoad))
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

		public IObservable<Unit> SetPassword(string identity, string password, bool expirePassword = true) => Observable.Start(() =>
        {
            GetUser(identity).Wait().Principal.SetPassword(password);

            DoActionOnAllDCs(x =>
            {
                var user = UserPrincipal.FindByIdentity(new PrincipalContext(ContextType.Domain, x.Name), identity);
                if (user == null) throw new ArgumentException(UserNotFoundMessage, nameof(identity));

                if (expirePassword) user.ExpirePasswordNow();
                user.UnlockAccount();

                return Unit.Default;
            }).Wait();
        });

        public IObservable<Unit> ExpirePassword(string identity) => Observable.Start(() =>
        {
            DoActionOnAllDCs(x =>
            {
                var user = UserPrincipal.FindByIdentity(new PrincipalContext(ContextType.Domain, x.Name), identity);
                if (user == null) throw new ArgumentException(UserNotFoundMessage, nameof(identity));

                user.ExpirePasswordNow();

                return Unit.Default;
            }).Wait();
        });

        public IObservable<Unit> UnlockUser(string identity) => Observable.Start(() =>
        {
            DoActionOnAllDCs(x =>
            {
                var user = UserPrincipal.FindByIdentity(new PrincipalContext(ContextType.Domain, x.Name), identity);
                if (user == null) throw new ArgumentException(UserNotFoundMessage, nameof(identity));

                user.UnlockAccount();

                return Unit.Default;
            }).Wait();
        });

		public IObservable<Unit> SetEnabled(string identity, bool enabled) => Observable.Start(() =>
		{
			DoActionOnAllDCs(x =>
			{
				var user = UserPrincipal.FindByIdentity(new PrincipalContext(ContextType.Domain, x.Name), identity);
				if (user == null) throw new ArgumentException(UserNotFoundMessage, nameof(identity));

				user.Enabled = enabled;
				user.Save();

				return Unit.Default;
			}).Wait();
		});

		public IObservable<LockoutInfo> GetLockoutInfo(string identity) => DoActionOnAllDCs(x =>
		{
			var user = UserPrincipal.FindByIdentity(new PrincipalContext(ContextType.Domain, x.Name), identity);

			if (user == null) return new LockoutInfo { DomainControllerName = x.Name };

			return new LockoutInfo
			{
				DomainControllerName = x.Name.Split('.').FirstOrDefault(),
				UserState = user.IsAccountLockedOut(),
				BadPasswordCount = user.BadLogonCount,
				LastBadPassword = user.LastBadPasswordAttempt,
				PasswordLastSet = user.LastPasswordSet,
				LockoutTime = user.AccountLockoutTime
			};
		});

        private IObservable<TResult> DoActionOnAllDCs<TResult>(Func<DomainController, TResult> action) => Observable.Start(() => Domain.GetCurrentDomain().DomainControllers)
            .SelectMany(dcs =>
            {
                var dcList = new List<DomainController>();
                foreach (DomainController dc in dcs) dcList.Add(dc);
                return dcList.ToObservable();
            })
            .SelectMany(dc => Observable.Start(() => action(dc)).CatchAndReturn(default(TResult)));
    }
}
