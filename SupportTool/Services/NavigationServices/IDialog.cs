using System;
using System.Threading.Tasks;

namespace SupportTool.Services.NavigationServices
{
	public interface IDialog
	{
		Task Opening(Action close, object parameter);
	}

	public interface IDialog<TResult>
	{
		Task Opening(Action<TResult> close, object parameter);
		IObservable<TResult> Result { get; }
	}
}
