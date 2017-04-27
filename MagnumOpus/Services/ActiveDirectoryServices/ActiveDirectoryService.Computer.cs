using MagnumOpus.Models;
using System;
using System.DirectoryServices.AccountManagement;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace MagnumOpus.Services.ActiveDirectoryServices
{
	public partial class ActiveDirectoryService
    {
        public IObservable<ComputerObject> GetComputer(string identity, IScheduler scheduler = null) => Observable.Start(() =>
        {
            var cp = ComputerPrincipal.FindByIdentity(_principalContext, identity);
            return cp != null ? new ComputerObject(cp) : null;
        }, scheduler ?? TaskPoolScheduler.Default);
    }
}
