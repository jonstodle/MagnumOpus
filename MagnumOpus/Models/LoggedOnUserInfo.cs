using System;

namespace MagnumOpus.Models
{
    public class LoggedOnUserInfo
	{
		public string Username { get; set; }
        public string SessionName { get; set; }
        public int SessionID { get; set; }
        public string State { get; set; }
        public TimeSpan IdleTime { get; set; }
        public DateTimeOffset LogonTime { get; set; }
    }
}
