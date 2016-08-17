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
        public ComputerObject(ComputerPrincipal principal) : base(principal) { }

        public string CN => directoryEntry.Properties.Get<string>("cn");
    }
}
