using SupportTool.Models;
using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupportTool.Services.ActiveDirectoryServices
{
    public partial class ActiveDirectoryService
    {
        public static ActiveDirectoryService Current { get; } = new ActiveDirectoryService();



        PrincipalContext principalContext;

        private ActiveDirectoryService()
        {
            principalContext = new PrincipalContext(ContextType.Domain);
        }



        public IObservable<Principal> GetPrincipal(string identity) => Observable.Start(() => Principal.FindByIdentity(principalContext, identity));
    }
}
