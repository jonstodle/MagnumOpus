﻿using System;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using MagnumOpus.ActiveDirectory;
using MagnumOpus.Settings;
using MagnumOpus.User;
using Splat;

namespace MagnumOpus.Computer
{
    public class ComputerObject : ActiveDirectoryObject<ComputerPrincipal>
    {
        public ComputerObject(ComputerPrincipal principal) : base(principal) { }



        public string OperatingSystem => DirectoryEntry.Properties.Get<string>("operatingsystem");

        public string ServicePack => DirectoryEntry.Properties.Get<string>("operatingsystemservicepack");

        public string Company => Locator.Current.GetService<SettingsFacade>().ComputerCompanyOus.FirstOrDefault(companyKVPair => Principal.DistinguishedName.Contains(companyKVPair.Key)).Value ?? "";


        
        public static IObservable<LoggedOnUserInfo> GetLoggedInUsers(string hostName, IScheduler scheduler = null) => Observable.Create<LoggedOnUserInfo>(
            observer =>
                (scheduler ?? TaskPoolScheduler.Default).Schedule(() =>
                {
                    var conOptions = new ConnectionOptions()
                    {
                        Impersonation = ImpersonationLevel.Impersonate,
                        EnablePrivileges = true
                    };
                    var scope = new ManagementScope($"\\\\{hostName}\\ROOT\\CIMV2", conOptions);
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

        public IObservable<string> GetIPAddress(IScheduler scheduler = null) => Observable.Start(() =>
            Dns.GetHostEntry(CN).AddressList.First(ipAddress => ipAddress.AddressFamily == AddressFamily.InterNetwork).ToString(), scheduler ?? TaskPoolScheduler.Default)
            .CatchAndReturn("");

        public IObservable<LoggedOnUserInfo> GetLoggedInUsers(IScheduler scheduler = null) =>
            GetLoggedInUsers(CN, scheduler);

        public IObservable<UserObject> GetManagedBy() => Observable.Return(DirectoryEntry.Properties.Get<string>("managedby"))
            .SelectMany(username =>
            {
                if (username == null) return Observable.Return<UserObject>(null);
                else return Locator.Current.GetService<ADFacade>().GetUser(username);
            })
            .CatchAndReturn(null)
            .Take(1);
    }
}
