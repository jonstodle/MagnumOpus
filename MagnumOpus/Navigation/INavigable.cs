using System.Threading.Tasks;

namespace MagnumOpus.Navigation
{
	public interface INavigable
    {
        Task OnNavigatedTo(object parameter);
        Task OnNavigatingFrom();
    }
}
