using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SupportTool.Helpers;

namespace SupportTool.Models
{
    public class UserObject : ActiveDirectoryObject<UserPrincipal>
    {
        public UserObject(UserPrincipal principal) : base(principal) { }

        public string Company => directoryEntry.Properties.Get<string>("company");

        public string ProfilePath => directoryEntry.Properties.Get<string>("profilepath");
    }
}
