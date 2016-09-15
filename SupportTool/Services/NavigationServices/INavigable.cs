using System.Threading.Tasks;

namespace SupportTool.Services.NavigationServices
{
	public interface INavigable
    {
        Task OnNavigatedTo(object parameter);
        Task OnNavigatingFrom();
    }
}
