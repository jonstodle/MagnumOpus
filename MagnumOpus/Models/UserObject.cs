using MagnumOpus.Services.ActiveDirectoryServices;
using System;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Reactive.Linq;

namespace MagnumOpus.Models
{
	public class UserObject : ActiveDirectoryObject<UserPrincipal>
    {
        public UserObject(UserPrincipal principal) : base(principal) { }



        public string Name => _principal.DisplayName ?? _principal.Name;

        public string JobTitle => _directoryEntry.Properties.Get<string>("title");

        public string Department => _directoryEntry.Properties.Get<string>("department");

        public string Company => _directoryEntry.Properties.Get<string>("company");

        public string ProfilePath => _directoryEntry.Properties.Get<string>("profilepath") ?? "";

		public string HomeDirectory => _directoryEntry.Properties.Get<string>("homedirectory") ?? "";

        public IObservable<UserObject> GetManager() => Observable.Return(_directoryEntry.Properties.Get<string>("manager"))
            .SelectMany(username =>
            {
                if (username == null) return Observable.Empty<UserObject>();
                else return ActiveDirectoryService.Current.GetUser(username);
            })
            .Catch(Observable.Empty<UserObject>())
            .Take(1);

        public IObservable<UserObject> GetDirectReports() => _directoryEntry.Properties["directreports"].ToEnumerable<string>().ToObservable()
            .SelectMany(username =>
            {
                if (username == null) return Observable.Empty<UserObject>();
                else return ActiveDirectoryService.Current.GetUser(username).Catch(Observable.Empty<UserObject>());
            });
    }
}
