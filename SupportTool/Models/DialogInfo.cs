using ReactiveUI;
using SupportTool.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace SupportTool.Models
{
	public struct DialogInfo
	{
		public Control Control { get; private set; }
		public object Parameter { get; private set; }

		public DialogInfo(Control control, object parameter)
		{
			Control = control;
			Parameter = parameter;
		}
	}



	public static class DialogInfoHelpers
	{
		public static IDisposable RegisterDialogHandler(this Interaction<DialogInfo, Unit> source, Grid parent) => source.RegisterHandler(interaction => 
		{
			new ModalControl(parent, interaction.Input.Control, interaction.Input.Parameter);
			interaction.SetOutput(Unit.Default);
		});
	}
}
