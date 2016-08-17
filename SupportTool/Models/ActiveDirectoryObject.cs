using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupportTool.Models
{
    public class ActiveDirectoryObject<T> where T : Principal
    {
        protected T principal;
        protected DirectoryEntry directoryEntry;



        public ActiveDirectoryObject(T principal)
        {
            this.principal = principal;
            directoryEntry = principal.GetUnderlyingObject() as DirectoryEntry;
        }



        public T Principal => principal;
    }
}
