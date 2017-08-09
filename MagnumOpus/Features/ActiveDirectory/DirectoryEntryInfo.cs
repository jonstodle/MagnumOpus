using System;
using System.Linq;
using System.DirectoryServices;

namespace MagnumOpus.ActiveDirectory
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



        public string Path { get; }
        public string CN { get; }
        public string DistinguishedName { get; }
        public string SamAccountName { get; }
        public string Name { get; }
        public string Company { get; }
        public string ObjectType { get; }



        public int CompareTo(DirectoryEntryInfo other) => string.Compare(CN, other.CN, StringComparison.OrdinalIgnoreCase);

		public int CompareTo(object obj)
		{
			var other = obj as DirectoryEntryInfo;

			return obj == null ? -1 : CompareTo(other);
		}
	}
}
