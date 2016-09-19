using ReactiveUI;
using SupportTool.Models;
using System;
using System.Linq;
using System.Reactive.Linq;

namespace SupportTool.ViewModels
{
	public class UserDetailsViewModel : ReactiveObject
    {
        private readonly ObservableAsPropertyHelper<bool> isAccountLocked;
        private readonly ObservableAsPropertyHelper<TimeSpan> passwordAge;
        private UserObject user;



        public UserDetailsViewModel()
        {
			this
				.WhenAnyValue(x => x.User)
				.WhereNotNull()
                .Select(x => x.Principal.IsAccountLockedOut())
                .ToProperty(this, x => x.IsAccountLocked, out isAccountLocked);

            this
                .WhenAnyValue(x => x.User)
                .Where(x => x != null && x.Principal.LastPasswordSet != null)
                .Select(x => (TimeSpan)(DateTime.Now - x.Principal.LastPasswordSet.Value.ToLocalTime()))
                .ToProperty(this, x => x.PasswordAge, out passwordAge);
        }



        public bool IsAccountLocked => isAccountLocked.Value;

        public TimeSpan PasswordAge => passwordAge.Value;

        public UserObject User
        {
            get { return user; }
            set { this.RaiseAndSetIfChanged(ref user, value); }
        }
    }
}
