using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupportTool.Models
{
    public class GroupObject : ActiveDirectoryObject<GroupPrincipal>
    {
        public GroupObject(GroupPrincipal principal) : base(principal) { }
    }
}
