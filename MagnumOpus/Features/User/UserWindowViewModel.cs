using ReactiveUI;
using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using MagnumOpus.ActiveDirectory;
using MagnumOpus.Dialog;
using MagnumOpus.Navigation;
using Splat;

namespace MagnumOpus.User
{
	public class UserWindowViewModel : ViewModelBase, INavigable
	{
		public UserWindowViewModel()
		{
			SetUser = ReactiveCommand.CreateFromObservable<string, UserObject>(identity => Locator.Current.GetService<ADFacade>().GetUser(identity));

            _user = SetUser
                .ToProperty(this, vm => vm.User);

            this.WhenActivated(disposables =>
            {
                SetUser
                    .ThrownExceptions
                    .SelectMany(ex => _messages.Handle(new MessageInfo(MessageType.Error, ex.Message, "Could not load user")))
                    .Subscribe()
                    .DisposeWith(disposables);
            });
		}



		public ReactiveCommand<string, UserObject> SetUser { get; }
        public UserObject User => _user.Value;



		public Task OnNavigatedTo(object parameter)
		{
			if (parameter is string s)
			{
				Observable.Return(s)
					.InvokeCommand(SetUser);
			}
			else if (parameter is Tuple<string,string> t)
			{
				Observable.Return(t.Item1)
					.InvokeCommand(SetUser);

                Observable.Return(t.Item2)
                    .Delay(TimeSpan.FromSeconds(1))
                    .ObserveOnDispatcher()
                    .Subscribe(computerName => MessageBus.Current.SendMessage(computerName, ApplicationActionRequest.SetLocalProfileComputerName));
			}

			return Task.FromResult<object>(null);
		}

		public Task OnNavigatingFrom() => Task.FromResult<object>(null);



        private readonly ObservableAsPropertyHelper<UserObject> _user;
    }
}
