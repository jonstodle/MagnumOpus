using System;
using System.Linq;
using System.DirectoryServices;

namespace MagnumOpus.Models
{
	public class DirectoryEntryInfo : IComparable, IComparable<DirectoryEntryInfo>
	{
		public DirectoryEntryInfo(DirectoryEntry directoryEntry)
		{
			Path = directoryEntry.Path;
			CN = directoryEntry.Properties["cn"].Value?.ToString();
			DistinguishedName = directoryEntry.Properties["distinguishedname"].Value?.ToString();
			SamAccountName = directoryEntry.Properties["samaccountname"].Value?.ToString();
			Name = directoryEntry.Properties["name"].Value?.ToString();
            Company = directoryEntry.Properties.Get<string>("company");
			ObjectType = directoryEntry.Properties["objectcategory"].Value?.ToString().Split(',').FirstOrDefault()?.Split('=').Skip(1).FirstOrDefault();

			directoryEntry.Dispose();
		}



        public string Path { get; private set; }
        public string CN { get; private set; }
        public string DistinguishedName { get; private set; }
        public string SamAccountName { get; private set; }
        public string Name { get; private set; }
        public string Company { get; private set; }
        public string ObjectType { get; private set; }



        public int CompareTo(DirectoryEntryInfo other) => CN.CompareTo(other.CN);

		public int CompareTo(object obj)
		{
			var other = obj as DirectoryEntryInfo;

			return obj == null ? -1 : CompareTo(other);
		}
	}
}
