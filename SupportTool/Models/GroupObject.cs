using System.DirectoryServices.AccountManagement;

namespace SupportTool.Models
{
	public class GroupObject : ActiveDirectoryObject<GroupPrincipal>
    {
        public GroupObject(GroupPrincipal principal) : base(principal) { }
    }
}
