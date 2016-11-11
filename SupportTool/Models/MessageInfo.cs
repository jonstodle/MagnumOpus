using SupportTool.Controls;
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
		public DialogButtonInfo[] Buttons { get; private set; }

		public MessageInfo(string message, string caption = "")
		{
			Message = message;
			Caption = caption;
			Buttons = null;
		}

		public MessageInfo(string message, string caption, params DialogButtonInfo[] buttons) : this(message, caption)
		{
			Buttons = buttons;
		}

		public MessageInfo(string message, string caption, params string[] buttons) : this(message, caption)
		{
			Buttons = buttons.Select(x => new DialogButtonInfo(x)).ToArray();
		}

		public static MessageInfo PasswordSetMessageInfo(string password) => new MessageInfo($"New password is: {password}\nMust be changed at next logon", "Password set");

		public static MessageInfo PasswordSetErrorMessageInfo(string message = null) => new MessageInfo(message ?? $"Could not set password", "Password not set");
	}
}
