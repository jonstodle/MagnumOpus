using SupportTool.Helpers;
using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupportTool.Models
{
    public class ComputerObject : ActiveDirectoryObject<ComputerPrincipal>
    {
        public ComputerObject(ComputerPrincipal principal) : base(principal)
        {

        }



        public string Company
        {
            get
            {
                var dn = principal.DistinguishedName;

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
    }
}
