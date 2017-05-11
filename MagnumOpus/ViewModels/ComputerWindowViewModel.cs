using ReactiveUI;
using MagnumOpus.Models;
using MagnumOpus.Services.ActiveDirectoryServices;
using MagnumOpus.Services.NavigationServices;
using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace MagnumOpus.ViewModels
{
	public class ComputerWindowViewModel : ViewModelBase, INavigable
	{
		public ComputerWindowViewModel()
		{
			SetComputer = ReactiveCommand.CreateFromObservable<string, ComputerObject>(identity => ActiveDirectoryService.Current.GetComputer(identity));

            _computer = SetComputer
				.ToProperty(this, x => x.Computer);

            this.WhenActivated(disposables =>
            {
                SetComputer
                    .ThrownExceptions
                    .SelectMany(ex => _messages.Handle(new MessageInfo(MessageType.Error, ex.Message, "Could not open computer")))
                    .Subscribe()
                    .DisposeWith(disposables);
            });
		}



		public ReactiveCommand<string, ComputerObject> SetComputer { get; private set; }

		public ComputerObject Computer => _computer.Value;



		public Task OnNavigatedTo(object parameter)
		{
			if (parameter is string s)
			{
				Observable.Return(s)
					.InvokeCommand(SetComputer);
			}

			return Task.FromResult<object>(null);
		}

		public Task OnNavigatingFrom() => Task.FromResult<object>(null);



        private readonly ObservableAsPropertyHelper<ComputerObject> _computer;
    }
}
