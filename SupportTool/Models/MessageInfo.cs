using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupportTool.Models
{
	public struct MessageInfo
	{
		public string Caption { get; private set; }
		public string Message { get; private set; }

		public MessageInfo(string message, string caption = "")
		{
			Message = message;
			Caption = caption;
		}

		public static MessageInfo PasswordSetMessageInfo(string password) => new MessageInfo("Password set", $"New password is: {password}\nMust be changed at next logon");

		public static MessageInfo PasswordSetErrorMessageInfo(string message = null) => new MessageInfo("Password not set", message ?? $"Could not set password");
	}
}
