using MagnumOpus.Services.ActiveDirectoryServices;
using MagnumOpus.Services.SettingsServices;
using System;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace MagnumOpus.Models
{
    public class ComputerObject : ActiveDirectoryObject<ComputerPrincipal>
    {
        public ComputerObject(ComputerPrincipal principal) : base(principal) { }



        public string OperatingSystem => _directoryEntry.Properties.Get<string>("operatingsystem");

        public string ServicePack => _directoryEntry.Properties.Get<string>("operatingsystemservicepack");

        public string Company => SettingsService.Current.ComputerCompanyOus.FirstOrDefault(companyKVPair => _principal.DistinguishedName.Contains(companyKVPair.Key)).Value ?? "";



        public IObservable<string> GetIPAddress(IScheduler scheduler = null) => Observable.Start(() =>
            Dns.GetHostEntry(CN).AddressList.First(ipAddress => ipAddress.AddressFamily == AddressFamily.InterNetwork).ToString(), scheduler ?? TaskPoolScheduler.Default)
            .CatchAndReturn("");

        public IObservable<LoggedOnUserInfo> GetLoggedInUsers(IScheduler scheduler = null) => Observable.Create<LoggedOnUserInfo>(
            observer =>
                (scheduler ?? TaskPoolScheduler.Default).Schedule(() =>
                    {
                        var conOptions = new ConnectionOptions()
                        {
                            Impersonation = ImpersonationLevel.Impersonate,
                            EnablePrivileges = true
                        };
                        var scope = new ManagementScope($"\\\\{CN}\\ROOT\\CIMV2", conOptions);
                        scope.Connect();

                        var query = new ObjectQuery("SELECT * FROM Win32_Process where name='explorer.exe'");
                        var searcher = new ManagementObjectSearcher(scope, query);

                        foreach (ManagementObject item in searcher.Get())
                        {
                            var argsArray = new string[] { "" };
                            item.InvokeMethod("GetOwner", argsArray);
                            var hasSessionID = int.TryParse(item["sessionID"].ToString(), out int sessionID);
                            observer.OnNext(new LoggedOnUserInfo { Username = argsArray[0], SessionID = hasSessionID ? sessionID : -1 });
                        }

                        observer.OnCompleted();
                    }));

        public IObservable<UserObject> GetManagedBy() => Observable.Return(_directoryEntry.Properties.Get<string>("managedby"))
            .SelectMany(username =>
            {
                if (username == null) return Observable.Return<UserObject>(null);
                else return ActiveDirectoryService.Current.GetUser(username);
            })
            .CatchAndReturn(null)
            .Take(1);
    }
}
