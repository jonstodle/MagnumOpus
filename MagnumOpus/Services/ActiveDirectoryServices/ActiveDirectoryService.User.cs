using MagnumOpus.Models;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace MagnumOpus.Services.ActiveDirectoryServices
{
	public partial class ActiveDirectoryService
    {
        public IObservable<UserObject> GetUser(string identity, IScheduler scheduler = null) => Observable.Start(() =>
        {
            var up = UserPrincipal.FindByIdentity(_principalContext, identity);
            return up != null ? new UserObject(up) : null;
        }, scheduler ?? TaskPoolScheduler.Default);

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

        private TimeSpan? _domainMaxPasswordAge = null;
        public TimeSpan DomainMaxPasswordAge
        {
            get
            {
                if (_domainMaxPasswordAge == null)
                {
                    using(var directoryEntry = GetDomainDirectoryEntry())
                    using (var searcher = new DirectorySearcher(directoryEntry, "(objectCategory=domainDNS)"))
                    {
                        _domainMaxPasswordAge = TimeSpan.FromTicks(searcher.FindOne().Properties.Get<long>("maxPwdAge")).Duration();
                    }
                }
                return (TimeSpan)_domainMaxPasswordAge;
            }
        }

        public IObservable<TimeSpan> GetMaxPasswordAge(string identity, IScheduler scheduler = null) => GetUsers(identity, "msDS-ResultantPSO")
            .SubscribeOn(scheduler ?? RxApp.TaskpoolScheduler)
            .Take(1)
            .Select(userDe => userDe.Properties.Get<long>("msDS-ResultantPSO"))
            .Select(maxAge => TimeSpan.FromTicks(Math.Abs(maxAge)))
            .CatchAndReturn(DomainMaxPasswordAge);

        public IObservable<Unit> SetPassword(string identity, string password, bool expirePassword = true, IScheduler scheduler = null) => Observable.Start(() =>
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
        }, scheduler ?? TaskPoolScheduler.Default);

        public IObservable<Unit> ExpirePassword(string identity, IScheduler scheduler = null) => Observable.Start(() =>
        {
            DoActionOnAllDCs(x =>
            {
                var user = UserPrincipal.FindByIdentity(new PrincipalContext(ContextType.Domain, x.Name), identity);
                if (user == null) throw new ArgumentException(UserNotFoundMessage, nameof(identity));

                user.ExpirePasswordNow();

                return Unit.Default;
            }).Wait();
        }, scheduler ?? TaskPoolScheduler.Default);

        public IObservable<Unit> UnlockUser(string identity, IScheduler scheduler = null) => Observable.Start(() =>
        {
            DoActionOnAllDCs(x =>
            {
                var user = UserPrincipal.FindByIdentity(new PrincipalContext(ContextType.Domain, x.Name), identity);
                if (user == null) throw new ArgumentException(UserNotFoundMessage, nameof(identity));

                user.UnlockAccount();

                return Unit.Default;
            }).Wait();
        }, scheduler ?? TaskPoolScheduler.Default);

		public IObservable<Unit> SetEnabled(string identity, bool enabled, IScheduler scheduler = null) => Observable.Start(() =>
		{
			DoActionOnAllDCs(x =>
			{
				var user = UserPrincipal.FindByIdentity(new PrincipalContext(ContextType.Domain, x.Name), identity);
				if (user == null) throw new ArgumentException(UserNotFoundMessage, nameof(identity));

				user.Enabled = enabled;
				user.Save();

				return Unit.Default;
			}).Wait();
		}, scheduler ?? TaskPoolScheduler.Default);

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

        private IObservable<TResult> DoActionOnAllDCs<TResult>(Func<DomainController, TResult> action, IScheduler scheduler = null) => Observable.Start(() => Domain.GetCurrentDomain().DomainControllers, scheduler ?? TaskPoolScheduler.Default)
            .SelectMany(dcs => dcs.ToGeneric<DomainController>().ToObservable())
            .SelectMany(dc => Observable.Start(() => action(dc), NewThreadScheduler.Default));
    }
}
