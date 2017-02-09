using ReactiveUI;
using SupportTool.Models;
using SupportTool.Services.ActiveDirectoryServices;
using SupportTool.Services.NavigationServices;
using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace SupportTool.ViewModels
{
	public class UserWindowViewModel : ViewModelBase, INavigable
	{
		public UserWindowViewModel()
		{
			_setUser = ReactiveCommand.CreateFromObservable<string, UserObject>(identity => ActiveDirectoryService.Current.GetUser(identity));

            _user = _setUser
                .ToProperty(this, x => x.User);

            this.WhenActivated(disposables =>
            {
                _setUser
                .ThrownExceptions
                .Subscribe(async ex => await _errorMessages.Handle(new MessageInfo(ex.Message)))
                .DisposeWith(disposables);
            });
		}



		public ReactiveCommand SetUser => _setUser;

		public UserObject User => _user.Value;



		public Task OnNavigatedTo(object parameter)
		{
			if (parameter is string s)
			{
				Observable.Return(s)
					.InvokeCommand(_setUser);
			}
			else if (parameter is Tuple<string,string> t)
			{
				Observable.Return(t.Item1)
					.InvokeCommand(_setUser);

                Observable.Return(t.Item2)
                    .Delay(TimeSpan.FromSeconds(1))
                    .ObserveOnDispatcher()
                    .Subscribe(x => MessageBus.Current.SendMessage(x, ApplicationActionRequest.SetLocalProfileComputerName.ToString()));
			}

			return Task.FromResult<object>(null);
		}

		public Task OnNavigatingFrom() => Task.FromResult<object>(null);



        private readonly ReactiveCommand<string, UserObject> _setUser;
        private readonly ObservableAsPropertyHelper<UserObject> _user;
    }
}
