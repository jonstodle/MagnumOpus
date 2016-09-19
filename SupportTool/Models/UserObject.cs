using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;

namespace SupportTool.Models
{
	public class UserObject : ActiveDirectoryObject<UserPrincipal>
    {
        public UserObject(UserPrincipal principal) : base(principal) { }

        public string Company => directoryEntry.Properties.Get<string>("company");

        public string ProfilePath => directoryEntry.Properties.Get<string>("profilepath");
    }
}
