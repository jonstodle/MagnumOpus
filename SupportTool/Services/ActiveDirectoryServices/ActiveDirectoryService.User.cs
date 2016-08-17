using SupportTool.Models;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupportTool.Services.ActiveDirectoryServices
{
    public partial class ActiveDirectoryService
    {
        public IObservable<UserObject> GetUser(string identity) => Observable.Start(() =>
        {
            var up = UserPrincipal.FindByIdentity(principalContext, identity);
            return up != null ? new UserObject(up) : null;
        });

        public IObservable<Unit> SetPassword(string identity, string password) => Observable.Start(() =>
        {
            var user = GetUser(identity).Wait();
            if (user == null) throw new ArgumentException(UserNotFoundMessage, nameof(identity));

            user.Principal.SetPassword(password);
            user.Principal.ExpirePasswordNow();
            user.Principal.UnlockAccount();
        });

        public IObservable<Unit> ExpirePassword(string identity) => Observable.Start(() =>
        {
            var user = GetUser(identity).Wait();
            if (user == null) throw new ArgumentException(UserNotFoundMessage, nameof(identity));

            user.Principal.ExpirePasswordNow();
        });

        public IObservable<Unit> UnlockUser(string identity) => Observable.Start(() =>
        {
            var user = GetUser(identity).Wait();
            if (user == null) throw new ArgumentException(UserNotFoundMessage, nameof(identity));

            user.Principal.UnlockAccount();

            //foreach (DomainController dc in Domain.GetCurrentDomain().DomainControllers)
            //{
            //    using (var searcher = dc.GetDirectorySearcher())
            //    {
            //        searcher.Filter = $"(samaccountname={user.Principal.SamAccountName})";
            //    }
            //}
        });
    }
}
