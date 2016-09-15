using SupportTool.Helpers;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;

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

        public PropertyValueCollection MemberOf => directoryEntry.Properties["memberof"];

        public string CN => directoryEntry.Properties.Get<string>("cn");
    }
}
