using System;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using MagnumOpus.User;

namespace MagnumOpus.ActiveDirectory
{
    public partial class ActiveDirectoryService
    {
        public IObservable<UserObject> GetUser(string identity, IScheduler scheduler = null) => Observable.Start(() =>
        {
            var up = UserPrincipal.FindByIdentity(_principalContext, identity);
            return up != null ? new UserObject(up) : null;
        }, scheduler ?? TaskPoolScheduler.Default);

        public IObservable<DirectoryEntry> GetUsers(string searchTerm, params string[] propertiesToLoad) => GetUsers(searchTerm, TaskPoolScheduler.Default, propertiesToLoad);

		public IObservable<DirectoryEntry> GetUsers(string searchTerm, IScheduler scheduler, params string[] propertiesToLoad) => Observable.Create<DirectoryEntry>(
            observer =>
                (scheduler ?? TaskPoolScheduler.Default).Schedule(() =>
		            {
			            using (var directoryEntry = GetDomainDirectoryEntry())
			            using (var searcher = new DirectorySearcher(directoryEntry, $"(&(objectCategory=user)({searchTerm}))", propertiesToLoad))
			            {
				            searcher.PageSize = 1000;

				            using (var results = searcher.FindAll())
				            {
					            foreach (SearchResult result in results)
					            {
						            observer.OnNext(result.GetDirectoryEntry());
					            }
				            }

				            observer.OnCompleted();
			            }
		            }));

        private TimeSpan? _domainMaxPasswordAge = null;
        public TimeSpan DomainMaxPasswordAge
        {
            get
            {
                if (_domainMaxPasswordAge == null)
                {
                    try
                    {
                        using (var directoryEntry = GetDomainDirectoryEntry())
                        using (var searcher = new DirectorySearcher(directoryEntry, "(objectCategory=domainDNS)"))
                        {
                            _domainMaxPasswordAge = TimeSpan.FromTicks(searcher.FindOne().Properties.Get<long>("maxPwdAge")).Duration();
                        }
                    }
                    catch
                    {
                        _domainMaxPasswordAge = TimeSpan.Zero;
                    }
                }
                return (TimeSpan)_domainMaxPasswordAge;
            }
        }

        public IObservable<TimeSpan> GetMaxPasswordAge(string identity, IScheduler scheduler = null) => GetUsers($"samaccountname={identity}", scheduler, "msDS-ResultantPSO")
            .Take(1)
            .Select(userDe => userDe.Properties.Get<long>("msDS-ResultantPSO"))
            .Select(maxAge => TimeSpan.FromTicks(Math.Abs(maxAge)))
            .Select(maxAge => maxAge != TimeSpan.Zero ? maxAge : throw new Exception($"{identity} has unvalid max password age"))
            .CatchAndReturn(DomainMaxPasswordAge);

        public IObservable<Unit> SetPassword(string identity, string password, bool expirePassword = true, IScheduler scheduler = null) => Observable.Start(() =>
        {
            GetUser(identity).Wait().Principal.SetPassword(password);

            DoActionOnAllDCs(domainController =>
            {
                var user = UserPrincipal.FindByIdentity(new PrincipalContext(ContextType.Domain, domainController.Name), identity);
                if (user == null) throw new ArgumentException(UserNotFoundMessage, nameof(identity));

                if (expirePassword) user.ExpirePasswordNow();
                user.UnlockAccount();

                return Unit.Default;
            }).Wait();
        }, scheduler ?? TaskPoolScheduler.Default);

        public IObservable<Unit> ExpirePassword(string identity, IScheduler scheduler = null) => Observable.Start(() =>
        {
            DoActionOnAllDCs(domainController =>
            {
                var user = UserPrincipal.FindByIdentity(new PrincipalContext(ContextType.Domain, domainController.Name), identity);
                if (user == null) throw new ArgumentException(UserNotFoundMessage, nameof(identity));

                user.ExpirePasswordNow();

                return Unit.Default;
            }).Wait();
        }, scheduler ?? TaskPoolScheduler.Default);

        public IObservable<Unit> UnlockUser(string identity, IScheduler scheduler = null) => Observable.Start(() =>
        {
            DoActionOnAllDCs(domainController =>
            {
                var user = UserPrincipal.FindByIdentity(new PrincipalContext(ContextType.Domain, domainController.Name), identity);
                if (user == null) throw new ArgumentException(UserNotFoundMessage, nameof(identity));

                user.UnlockAccount();

                return Unit.Default;
            }).Wait();
        }, scheduler ?? TaskPoolScheduler.Default);

		public IObservable<Unit> SetEnabled(string identity, bool enabled, IScheduler scheduler = null) => Observable.Start(() =>
		{
			DoActionOnAllDCs(domainController =>
			{
				var user = UserPrincipal.FindByIdentity(new PrincipalContext(ContextType.Domain, domainController.Name), identity);
				if (user == null) throw new ArgumentException(UserNotFoundMessage, nameof(identity));

				user.Enabled = enabled;
				user.Save();

				return Unit.Default;
			}).Wait();
		}, scheduler ?? TaskPoolScheduler.Default);

		public IObservable<LockoutInfo> GetLockoutInfo(string identity, IScheduler scheduler = null) => DoActionOnAllDCs(domainController =>
		{
			var user = UserPrincipal.FindByIdentity(new PrincipalContext(ContextType.Domain, domainController.Name), identity);

			if (user == null) return new LockoutInfo { DomainControllerName = domainController.Name };

			return new LockoutInfo
			{
				DomainControllerName = domainController.Name.Split('.').FirstOrDefault(),
				UserState = user.IsAccountLockedOut(),
				BadPasswordCount = user.BadLogonCount,
				LastBadPassword = user.LastBadPasswordAttempt,
				PasswordLastSet = user.LastPasswordSet,
				LockoutTime = user.AccountLockoutTime
			};
		}, scheduler);

        private IObservable<TResult> DoActionOnAllDCs<TResult>(Func<DomainController, TResult> action, IScheduler scheduler = null) => Observable.Start(() => Domain.GetCurrentDomain().DomainControllers, scheduler ?? TaskPoolScheduler.Default)
            .SelectMany(dcs => dcs.ToGeneric<DomainController>().ToObservable())
            .SelectMany(dc => Observable.Start(() => action(dc), NewThreadScheduler.Default));
    }
}
