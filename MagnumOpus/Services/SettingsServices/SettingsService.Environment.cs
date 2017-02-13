using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagnumOpus.Services.SettingsServices
{
    public partial class SettingsService
    {
        // Variables specific to the current environment

        public string SplunkUrl
        {
            get => Get("https://sd3-splunksh-03.sikt.sykehuspartner.no/en-us/app/splunk_app_windows_infrastructure/search?q=search%20eventtype%3Dmsad-account-lockout%20user%3D\"{0}\"%20dest_nt_domain%3D\"SIKT\"&earliest=-7d%40h&latest=now");
            set => Set(value);
        }

        public string SCCMPath
        {
            get => Get(@"C:\Program Files\SCCM Tools\SCCM Client Center\SMSCliCtrV2.exe");
            set => Set(value);
        }

        public string LogDirectoryPath
        {
            get => Get(@"C:\Logs\Magnum Opus");
            set => Set(value);
        }

        public IEnumerable<KeyValuePair<string, string>> ComputerCompanyOus
        {
            get => Get(new List<KeyValuePair<string, string>> {
                new KeyValuePair<string, string>("OU=AHUSHF", "AHUSHF"),
                new KeyValuePair<string, string>("OU=BET", "BET"),
                new KeyValuePair<string, string>("OU=BLEHF", "BLEHF"),
                new KeyValuePair<string, string>("OU=HSORHF", "HSORHF"),
                new KeyValuePair<string, string>("OU=HSP", "HSP"),
                new KeyValuePair<string, string>("OU=HSRHF", "HSRHF"),
                new KeyValuePair<string, string>("OU=MHH", "MHH"),
                new KeyValuePair<string, string>("OU=OUSHF", "OUSHF"),
                new KeyValuePair<string, string>("OU=PiVHF", "PiVHF"),
                new KeyValuePair<string, string>("OU=PKH", "PKH"),
                new KeyValuePair<string, string>("OU=PNO", "PNO"),
                new KeyValuePair<string, string>("OU=REV", "REV"),
                new KeyValuePair<string, string>("OU=RSHF", "RSHF"),
                new KeyValuePair<string, string>("OU=SA", "SA"),
                new KeyValuePair<string, string>("OU=SABHF", "SABHF"),
                new KeyValuePair<string, string>("OU=SBHF", "SBHF"),
                new KeyValuePair<string, string>("OU=SIHF", "SIHF"),
                new KeyValuePair<string, string>("OU=SiVHF", "SiVHF"),
                new KeyValuePair<string, string>("OU=SOHF", "SOHF"),
                new KeyValuePair<string, string>("OU=SP", "SP"),
                new KeyValuePair<string, string>("OU=SSHF", "SSHF"),
                new KeyValuePair<string, string>("OU=SSR", "SSR"),
                new KeyValuePair<string, string>("OU=STHF", "STHF"),
                new KeyValuePair<string, string>("OU=SUNHF", "SUNHF"),
                new KeyValuePair<string, string>("OU=VVHF", "VVHF"),
            });
            set => Set(value);
        }
    }
}
