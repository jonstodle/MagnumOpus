using System;
using System.Threading.Tasks;

namespace MagnumOpus.Services.NavigationServices
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
