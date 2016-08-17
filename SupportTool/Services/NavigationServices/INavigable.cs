using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupportTool.Services.NavigationServices
{
    public interface INavigable
    {
        Task OnNavigatedTo(object parameter);
        Task OnNavigatingFrom();
    }
}
