using System;
using System.Linq;
using System.DirectoryServices;

namespace SupportTool.Models
{
	public class DirectoryEntryInfo : IComparable, IComparable<DirectoryEntryInfo>
	{
		public string Path { get; set; }
		public string CN { get; set; }
		public string DistinguishedName { get; set; }
		public string SamAccountName { get; set; }
		public string Name { get; set; }
		public string ObjectType { get; set; }



		public DirectoryEntryInfo(DirectoryEntry directoryEntry)
		{
			Path = directoryEntry.Path;
			CN = directoryEntry.Properties["cn"].Value?.ToString();
			DistinguishedName = directoryEntry.Properties["distinguishedname"].Value?.ToString();
			SamAccountName = directoryEntry.Properties["samaccountname"].Value?.ToString();
			Name = directoryEntry.Properties["name"].Value?.ToString();
			ObjectType = directoryEntry.Properties["objectcategory"].Value?.ToString().Split(',').FirstOrDefault()?.Split('=').Skip(1).FirstOrDefault();

			directoryEntry.Dispose();
		}

		public int CompareTo(DirectoryEntryInfo other) => CN.CompareTo(other.CN);

		public int CompareTo(object obj)
		{
			var other = obj as DirectoryEntryInfo;

			return obj == null ? -1 : CompareTo(other);
		}
	}
}
