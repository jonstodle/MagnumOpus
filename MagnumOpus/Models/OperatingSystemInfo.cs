using System;
using System.Management;

namespace MagnumOpus.Models
{
	public class OperatingSystemInfo
	{
		public OperatingSystemInfo() { }

		public OperatingSystemInfo(ManagementObject managementObject)
		{
			var props = managementObject.Properties;

			Caption = props["caption"].Value?.ToString() ?? "";
			Version = props["version"].Value?.ToString() ?? "";
			BuildNumber = props["buildnumber"].Value?.ToString() ?? "";
			Architecture = props["osarchitecture"].Value?.ToString() ?? "";
			CSDVersion = props["csdversion"].Value?.ToString() ?? "";
			if(props["installdate"].Value != null) InstallDate = ManagementDateTimeConverter.ToDateTime(props["installdate"].Value?.ToString());
			if(props["lastbootuptime"].Value != null) LastBootTime = ManagementDateTimeConverter.ToDateTime(props["lastbootuptime"].Value?.ToString());

			managementObject.Dispose();
		}



        public string Caption { get; set; }
        public string Version { get; set; }
        public string BuildNumber { get; set; }
        public string Architecture { get; set; }
        public string CSDVersion { get; set; }
        public DateTime? InstallDate { get; set; }
        public DateTime? LastBootTime { get; set; }
    }
}
