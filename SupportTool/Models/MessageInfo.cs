using ReactiveUI;
using SupportTool.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

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

	public static class MessageInfoHelpers
	{
		public static IDisposable RegisterPromptHandler(this Interaction<MessageInfo, int> source, Grid parent) => source.RegisterHandler(async interaction => await new DialogControl(parent, interaction.Input.Caption.HasValue() ? interaction.Input.Caption : null, interaction.Input.Message, interaction.Input.Buttons).Result.Take(1));

		public static IDisposable RegisterPromptHandler(this IEnumerable<Interaction<MessageInfo, int>> source, Grid parent) => source.Aggregate(new CompositeDisposable(), (acc, input) =>
		{
			acc.Add(input.RegisterPromptHandler(parent));
			return acc;
		});

		public static IDisposable RegisterInfoHandler(this Interaction<MessageInfo, Unit> source, Grid parent) => source.RegisterHandler(async interaction =>
		{
			await DialogControl.InfoDialog(parent, interaction.Input.Caption.HasValue() ? interaction.Input.Caption : null, interaction.Input.Message).Result.Take(1);
			interaction.SetOutput(Unit.Default);
		});

		public static IDisposable RegisterInfoHandler(this IEnumerable<Interaction<MessageInfo, Unit>> source, Grid parent) => source.Aggregate(new CompositeDisposable(), (acc, input) =>
		{
			acc.Add(input.RegisterInfoHandler(parent));
			return acc;
		});

		public static IDisposable RegisterErrorHandler(this Interaction<MessageInfo, Unit> source, Grid parent) => source.RegisterHandler(async interaction =>
		{
			await DialogControl.ErrorDialog(parent, interaction.Input.Caption.HasValue() ? interaction.Input.Caption : null, interaction.Input.Message).Result.Take(1);
			interaction.SetOutput(Unit.Default);
		});

		public static IDisposable RegisterErrorHandler(this IEnumerable<Interaction<MessageInfo, Unit>> source, Grid parent) => source.Aggregate(new CompositeDisposable(), (acc, input) =>
		{
			acc.Add(input.RegisterErrorHandler(parent));
			return acc;
		});
	}
}
