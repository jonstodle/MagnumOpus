using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupportTool.Models
{
	public struct MessageInfo
	{
		public MessageType MessageType { get; private set; }
		public string Caption { get; private set; }
		public string Message { get; private set; }

		public MessageInfo(MessageType type, string message, string caption = "")
		{
			MessageType = type;
			Message = message;
			Caption = caption;
		}

		public static MessageInfo PasswordSetMessageInfo(string password) => new MessageInfo(MessageType.Info, "Password set", $"New password is: {password}\nMust be changed at next logon");

		public static MessageInfo PasswordSetErrorMessageInfo(string message = null) => new MessageInfo(MessageType.Error, "Password not set", message ?? $"Could not set password");
	}

	public enum MessageType
	{
		Info,
		Error
	}
}
