using System;
using System.Threading.Tasks;

namespace SupportTool.Services.NavigationServices
{
	public interface IDialog
	{
		Task Opening(object parameter);
	}

	public interface IDialog<TResult>
	{
		Task Opening(object parameter);
		IObservable<TResult> Result { get; }
	}
}
