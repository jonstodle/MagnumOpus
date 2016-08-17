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
        public IObservable<ComputerObject> GetComputer(string identity) => Observable.Start(() =>
        {
            var cp = ComputerPrincipal.FindByIdentity(principalContext, identity);
            return cp != null ? new ComputerObject(cp) : null;
        });
    }
}
