using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Controls;

namespace MagnumOpus.Dialog
{
    public struct MessageInfo
	{
		public MessageInfo(MessageType messageType, string message, string caption = "")
		{
            Type = messageType;
			Message = message;
			Caption = caption;
			Buttons = new[] { new DialogButtonInfo("OK", isDefault: true) };
		}

		public MessageInfo(MessageType messageType, string message, string caption, params DialogButtonInfo[] buttons) : this(messageType, message, caption)
		{
			Buttons = buttons;
		}

		public MessageInfo(MessageType messageType, string message, string caption, params string[] buttons) : this(messageType, message, caption)
		{
			Buttons = buttons.Select(buttonText => new DialogButtonInfo(buttonText)).ToArray();
		}



        public MessageType Type { get; private set; }
        public string Caption { get; private set; }
        public string Message { get; private set; }
        public DialogButtonInfo[] Buttons { get; private set; }

        public static MessageInfo PasswordSetMessageInfo(string password) => new MessageInfo(MessageType.Success, $"New password is: {password}\nMust be changed at next logon", "Password set");

		public static MessageInfo PasswordSetErrorMessageInfo(string message = null) => new MessageInfo(MessageType.Error, message ?? $"Could not set password", "Password not set");
	}



    public enum MessageType { Info, Question, Success, Warning, Error }



    public static class MessageInfoHelpers
	{
		public static IDisposable RegisterMessageHandler(this Interaction<MessageInfo, int> source, Grid parent) => source.RegisterHandler(async interaction => 
        {
            interaction.SetOutput(await new DialogControl(parent, interaction.Input).Result.Take(1));
        });

		public static IDisposable RegisterMessageHandler(this IEnumerable<Interaction<MessageInfo, int>> source, Grid parent) => source.Aggregate(new CompositeDisposable(), (acc, input) =>
		{
			acc.Add(input.RegisterMessageHandler(parent));
			return acc;
		});
	}
}
