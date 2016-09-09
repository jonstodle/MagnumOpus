using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SupportTool.Services.NavigationServices
{
	public interface IDialog
	{
		Task Opening(Action close, object parameter);
	}

	public interface IDialog<TResult>
	{
		Task Opening(Action<TResult> close, object parameter);
	}
}
