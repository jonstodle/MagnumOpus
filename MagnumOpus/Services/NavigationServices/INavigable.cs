using System.Threading.Tasks;

namespace MagnumOpus.Services.NavigationServices
{
	public interface INavigable
    {
        Task OnNavigatedTo(object parameter);
        Task OnNavigatingFrom();
    }
}
