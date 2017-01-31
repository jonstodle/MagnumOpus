using System;

namespace SupportTool.Models
{
    public class LockoutInfo
	{
		public string DomainControllerName { get; set; }
		public bool UserState { get; set; }
		public int BadPasswordCount { get; set; }
		public DateTime? LastBadPassword { get; set; }
		public DateTime? PasswordLastSet { get; set; }
		public DateTime? LockoutTime { get; set; }
		public string OriginalLock { get; set; }
	}
}
