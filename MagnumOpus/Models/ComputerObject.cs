using MagnumOpus.Services.ActiveDirectoryServices;
using MagnumOpus.Services.FileServices;
using MagnumOpus.Services.SettingsServices;
using System;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Linq;

namespace MagnumOpus.Models
{
    public class ComputerObject : ActiveDirectoryObject<ComputerPrincipal>
    {
        public ComputerObject(ComputerPrincipal principal) : base(principal) { }



        public string OperatingSystem => _directoryEntry.Properties.Get<string>("operatingsystem");

        public string ServicePack => _directoryEntry.Properties.Get<string>("operatingsystemservicepack");

        public string Company => SettingsService.Current.ComputerCompanyOus.FirstOrDefault(x => _principal.DistinguishedName.Contains(x.Key)).Value ?? "";



        public IObservable<string> GetIPAddress() => Observable.Start(() =>
            Dns.GetHostEntry(CN).AddressList.First(x => x.AddressFamily == AddressFamily.InterNetwork).ToString())
            .Catch(Observable.Return(""));

        public IObservable<LoggedOnUserInfo> GetLoggedInUsers() => Observable.Start(() =>
            ExecutionService.RunInCmdWithOuput(Path.Combine(ExecutionService.System32Path, "quser.exe"), $"/server:{CN}"))
            .SelectMany(x => x.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).Skip(1).ToObservable())
            .Select(x =>
            {
                var details = x.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                return new LoggedOnUserInfo
                {
                    Username = details[0].Replace(">", "").ToUpperInvariant(),
                    SessionName = details[1],
                    SessionID = int.TryParse(details[2], out var sid) ? sid : -1,
                    State = details[3],
                    IdleTime = TimeSpan.TryParse(!details[4].Contains(":") ? details[4].Insert(0, "0:") : details[4], out var it) ? it : TimeSpan.Zero,
                    LogonTime = DateTimeOffset.TryParse($"{details[5]} {details[6]}", out var lt) ? lt : DateTimeOffset.Now
                };
            });

        public IObservable<UserObject> GetManagedBy() => Observable.Return(_directoryEntry.Properties.Get<string>("managedby"))
            .SelectMany(x =>
            {
                if (x == null) return Observable.Return<UserObject>(null);
                else return ActiveDirectoryService.Current.GetUser(x);
            })
            .Catch(Observable.Return<UserObject>(null))
            .Take(1);
    }
}
