using ReactiveUI;
using SupportTool.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupportTool.ViewModels
{
    public class UserDetailsViewModel : ReactiveObject
    {
        private readonly ObservableAsPropertyHelper<bool> isAccountLocked;
        private UserObject user;



        public UserDetailsViewModel()
        {
            this
                .WhenAnyValue(x => x.User)
                .Where(x => x != null)
                .Select(x => x.Principal.IsAccountLockedOut())
                .ToProperty(this, x => x.IsAccountLocked, out isAccountLocked);
        }



        public bool IsAccountLocked => isAccountLocked.Value;

        public UserObject User
        {
            get { return user; }
            set { this.RaiseAndSetIfChanged(ref user, value); }
        }
    }
}
