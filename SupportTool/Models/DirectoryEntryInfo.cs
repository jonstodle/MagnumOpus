using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupportTool.Models
{
	public class DirectoryEntryInfo : IComparable, IComparable<DirectoryEntryInfo>
	{
		public string AdsPath { get; set; }
		public string CN { get; set; }
		public string DistinguishedName { get; set; }
		public string SamAccountName { get; set; }
		public string Name { get; set; }



		public DirectoryEntryInfo(DirectoryEntry directoryEntry)
		{
			AdsPath = directoryEntry.Properties["adspath"].Value?.ToString();
			CN = directoryEntry.Properties["cn"].Value?.ToString();
			DistinguishedName = directoryEntry.Properties["distinguishedname"].Value?.ToString();
			SamAccountName = directoryEntry.Properties["samaccountname"].Value?.ToString();
			Name = directoryEntry.Properties["name"].Value?.ToString();

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
