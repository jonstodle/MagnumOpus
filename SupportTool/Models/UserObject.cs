using SupportTool.Services.ActiveDirectoryServices;
using System;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Reactive.Linq;

namespace SupportTool.Models
{
	public class UserObject : ActiveDirectoryObject<UserPrincipal>
    {
        public UserObject(UserPrincipal principal) : base(principal) { }

		public string Name => $"{_directoryEntry.Properties.Get<string>("givenname")} {_directoryEntry.Properties.Get<string>("sn")} ({_directoryEntry.Properties.Get<string>("employeeid")})";

        public string Company => _directoryEntry.Properties.Get<string>("company");

        public string ProfilePath => _directoryEntry.Properties.Get<string>("profilepath");

		public string HomeDirectory => _directoryEntry.Properties.Get<string>("homedirectory");

        public IObservable<UserObject> GetManager() => Observable.Return(_directoryEntry.Properties.Get<string>("manager"))
            .SelectMany(x =>
            {
                if (x == null) return Observable.Return<UserObject>(null);
                else return ActiveDirectoryService.Current.GetUser(x);
            })
            .Catch(Observable.Return<UserObject>(null))
            .Take(1);
    }
}
