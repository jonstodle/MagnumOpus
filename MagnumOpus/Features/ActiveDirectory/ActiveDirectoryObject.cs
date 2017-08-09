using System;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;

namespace MagnumOpus.ActiveDirectory
{
	public class ActiveDirectoryObject<T> where T : Principal
    {
        public ActiveDirectoryObject(T principal)
        {
			Principal = principal;
            DirectoryEntry = principal.GetUnderlyingObject() as DirectoryEntry;
        }



        public T Principal { get; }

        public string CN => DirectoryEntry.Properties.Get<string>("cn");

		public DateTime? WhenCreated => DirectoryEntry.Properties.Get<DateTime?>("whencreated");


        protected DirectoryEntry DirectoryEntry;
    }
}
