using System;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Reactive.Linq;
using MagnumOpus.ActiveDirectory;
using Splat;

namespace MagnumOpus.User
{
	public class UserObject : ActiveDirectoryObject<UserPrincipal>
    {
        public UserObject(UserPrincipal principal) : base(principal) { }



        public string Name => Principal.DisplayName ?? Principal.Name;

        public string JobTitle => DirectoryEntry.Properties.Get<string>("title");

        public string Department => DirectoryEntry.Properties.Get<string>("department");

        public string Company => DirectoryEntry.Properties.Get<string>("company");

        public string ProfilePath => DirectoryEntry.Properties.Get<string>("profilepath") ?? "";

		public string HomeDirectory => DirectoryEntry.Properties.Get<string>("homedirectory") ?? "";

        public IObservable<UserObject> GetManager() => Observable.Return(DirectoryEntry.Properties.Get<string>("manager"))
            .SelectMany(username =>
            {
                if (username == null) return Observable.Empty<UserObject>();
                else return _adFacade.GetUser(username);
            })
            .Catch(Observable.Empty<UserObject>())
            .Take(1);

        public IObservable<UserObject> GetDirectReports() => DirectoryEntry.Properties["directreports"].ToEnumerable<string>().ToObservable()
            .SelectMany(username =>
            {
                if (username == null) return Observable.Empty<UserObject>();
                else return _adFacade.GetUser(username).Catch(Observable.Empty<UserObject>());
            });



        private readonly ADFacade _adFacade = Locator.Current.GetService<ADFacade>();
    }
}
