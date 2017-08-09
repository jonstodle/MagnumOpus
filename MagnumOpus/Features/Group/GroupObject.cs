using System;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Reactive.Linq;
using MagnumOpus.ActiveDirectory;
using MagnumOpus.User;

namespace MagnumOpus.Group
{
	public class GroupObject : ActiveDirectoryObject<GroupPrincipal>
    {
        public GroupObject(GroupPrincipal principal) : base(principal) { }



        public string Description { get => Principal.Description.HasValue() ? Principal.Description : ""; set => Principal.Description = value.HasValue() ? value : null; }

		public string Notes => DirectoryEntry.Properties.Get<string>("info");

        public IObservable<UserObject> GetManager() => Observable.Return(DirectoryEntry.Properties.Get<string>("manager"))
            .SelectMany(username =>
            {
                if (username == null) return Observable.Return<UserObject>(null);
                else return ActiveDirectoryService.Current.GetUser(username);
            })
            .CatchAndReturn(null)
            .Take(1);
    }
}
