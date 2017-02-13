using System;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;

namespace MagnumOpus.Models
{
	public class ActiveDirectoryObject<T> where T : Principal
    {
        public ActiveDirectoryObject(T principal)
        {
			_principal = principal;
            _directoryEntry = principal.GetUnderlyingObject() as DirectoryEntry;
        }



        public T Principal => _principal;

        public string CN => _directoryEntry.Properties.Get<string>("cn");

		public DateTime? WhenCreated => _directoryEntry.Properties.Get<DateTime?>("whencreated");



        protected T _principal;
        protected DirectoryEntry _directoryEntry;
    }
}
