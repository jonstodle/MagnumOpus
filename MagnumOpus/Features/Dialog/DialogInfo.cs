using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Controls;
using MagnumOpus.Modal;

namespace MagnumOpus.Dialog
{
	public struct DialogInfo
	{
		public DialogInfo(Control control, object parameter)
		{
			Control = control;
			Parameter = parameter;
		}



        public Control Control { get; private set; }
        public object Parameter { get; private set; }
    }



	public static class DialogInfoHelpers
	{
		public static IDisposable RegisterDialogHandler(this Interaction<DialogInfo, Unit> source, Grid parent) => source.RegisterHandler(async interaction => 
		{
			await new ModalControl(parent, interaction.Input.Control, interaction.Input.Parameter).Closed;
			interaction.SetOutput(Unit.Default);
		});

		public static IDisposable RegisterDialogHandler(this IEnumerable<Interaction<DialogInfo, Unit>> source, Grid parent) => source.Aggregate(new CompositeDisposable(), (acc, input) =>
		{
			acc.Add(input.RegisterDialogHandler(parent));
			return acc;
		});
	}
}
