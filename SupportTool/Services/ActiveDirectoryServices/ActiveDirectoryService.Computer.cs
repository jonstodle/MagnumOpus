using SupportTool.Models;
using System;
using System.DirectoryServices.AccountManagement;
using System.Reactive.Linq;

namespace SupportTool.Services.ActiveDirectoryServices
{
	public partial class ActiveDirectoryService
    {
        public IObservable<ComputerObject> GetComputer(string identity) => Observable.Start(() =>
        {
            var cp = ComputerPrincipal.FindByIdentity(_principalContext, identity);
            return cp != null ? new ComputerObject(cp) : null;
        });
    }
}
