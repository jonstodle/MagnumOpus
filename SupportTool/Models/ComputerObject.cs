using SupportTool.Services.ActiveDirectoryServices;
using System;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Management;
using System.Net;
using System.Reactive.Linq;

namespace SupportTool.Models
{
	public class ComputerObject : ActiveDirectoryObject<ComputerPrincipal>
    {
        public ComputerObject(ComputerPrincipal principal) : base(principal)
        {

        }



		public string OperatingSystem => _directoryEntry.Properties.Get<string>("operatingsystem");

		public string ServicePack => _directoryEntry.Properties.Get<string>("operatingsystemservicepack");

        public string Company
        {
            get
            {
                var dn = _principal.DistinguishedName;

                if (dn.Contains("OU=AHUSHF")) return "AHUSHF";
                if (dn.Contains("OU=BET")) return "BET";
                if (dn.Contains("OU=BFL")) return "BFL";
                if (dn.Contains("OU=BLEHF")) return "BLEHF";
                if (dn.Contains("OU=HSORHF")) return "HSORHF";
                if (dn.Contains("OU=HSP")) return "HSP";
                if (dn.Contains("OU=HSRHF")) return "HSRHF";
                if (dn.Contains("OU=MHH")) return "MHH";
                if (dn.Contains("OU=OUSHF")) return "OUSHF";
                if (dn.Contains("OU=PiVHF")) return "PiVHF";
                if (dn.Contains("OU=PKH")) return "PKH";
                if (dn.Contains("OU=PNO")) return "PNO";
                if (dn.Contains("OU=REV")) return "REV";
                if (dn.Contains("OU=RSHF")) return "RSHF";
                if (dn.Contains("OU=SA")) return "SA";
                if (dn.Contains("OU=SABHF")) return "SABHF";
                if (dn.Contains("OU=SBHF")) return "SBHF";
                if (dn.Contains("OU=SIHF")) return "SIHF";
                if (dn.Contains("OU=SiVHF")) return "SiVHF";
                if (dn.Contains("OU=SOHF")) return "SOHF";
                if (dn.Contains("OU=SP")) return "SP";
                if (dn.Contains("OU=SSHF")) return "SSHF";
                if (dn.Contains("OU=SSR")) return "SSR";
                if (dn.Contains("OU=STHF")) return "STHF";
                if (dn.Contains("OU=SUNHF")) return "SUNHF";
                if (dn.Contains("OU=VVHF")) return "VVHF";
                return "";
            }
        }



		public IObservable<string> GetIPAddress() => Observable.Start(() => 
			Dns.GetHostEntry(CN).AddressList.First(x => x.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).ToString())
			.Catch(Observable.Return(""));

		public IObservable<LoggedOnUserInfo> GetLoggedInUsers() => Observable.Create<LoggedOnUserInfo>(observer =>
		{
			var disposed = false;

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
				var argsArray = new string[] { string.Empty };
				item.InvokeMethod("GetOwner", argsArray);
				var hasSessionID = int.TryParse(item["sessionID"].ToString(), out int sessionID);
				if (disposed) break;
				observer.OnNext(new LoggedOnUserInfo { Username = argsArray[0], SessionID = hasSessionID ? sessionID : -1 });
			}

			observer.OnCompleted();
			return () => disposed = true;
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
