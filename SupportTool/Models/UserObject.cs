using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;

namespace SupportTool.Models
{
	public class UserObject : ActiveDirectoryObject<UserPrincipal>
    {
        public UserObject(UserPrincipal principal) : base(principal) { }

		public string Name => $"{_directoryEntry.Properties.Get<string>("givenname")} {_directoryEntry.Properties.Get<string>("sn")} ({_directoryEntry.Properties.Get<string>("employeeid")})";

        public string Company => _directoryEntry.Properties.Get<string>("company");

        public string ProfilePath => _directoryEntry.Properties.Get<string>("profilepath");

		public string HomeDirectory => _directoryEntry.Properties.Get<string>("homedirectory");
    }
}
