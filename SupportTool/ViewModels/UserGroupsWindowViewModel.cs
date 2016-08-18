using ReactiveUI;
using SupportTool.Models;
using SupportTool.Services.ActiveDirectoryServices;
using SupportTool.Services.NavigationServices;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupportTool.ViewModels
{
    public class UserGroupsWindowViewModel : ReactiveObject, INavigable
    {
        private readonly ReactiveList<SearchResult> groups;
        private UserObject user;



        public UserGroupsWindowViewModel()
        {

        }



        public UserObject User
        {
            get { return user; }
            set { this.RaiseAndSetIfChanged(ref user, value); }
        }




        public async Task OnNavigatedTo(object parameter)
        {
            if (parameter is string)
            {
                User = await ActiveDirectoryService.Current.GetUser(parameter as string);
            }
        }

        public async Task OnNavigatingFrom() => await Task.FromResult<object>(null);
    }
}
