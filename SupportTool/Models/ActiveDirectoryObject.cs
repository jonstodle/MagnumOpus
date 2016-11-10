using System;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;

namespace SupportTool.Models
{
	public class ActiveDirectoryObject<T> where T : Principal
    {
        protected T _principal;
        protected DirectoryEntry _directoryEntry;



        public ActiveDirectoryObject(T principal)
        {
			_principal = principal;
            _directoryEntry = principal.GetUnderlyingObject() as DirectoryEntry;
        }



        public T Principal => _principal;

        public PropertyValueCollection MemberOf => _directoryEntry.Properties["memberof"];

        public string CN => _directoryEntry.Properties.Get<string>("cn");

		public DateTime? WhenCreated => _directoryEntry.Properties.Get<DateTime?>("whencreated");
    }
}
