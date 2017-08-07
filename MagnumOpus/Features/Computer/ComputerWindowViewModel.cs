using ReactiveUI;
using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using MagnumOpus.ActiveDirectory;
using MagnumOpus.Dialog;
using MagnumOpus.Navigation;

namespace MagnumOpus.Computer
{
	public class ComputerWindowViewModel : ViewModelBase, INavigable
	{
		public ComputerWindowViewModel()
		{
			SetComputer = ReactiveCommand.CreateFromObservable<string, ComputerObject>(identity => ActiveDirectoryService.Current.GetComputer(identity));

            _computer = SetComputer
				.ToProperty(this, vm => vm.Computer);

            this.WhenActivated(disposables =>
            {
                SetComputer
                    .ThrownExceptions
                    .SelectMany(ex => _messages.Handle(new MessageInfo(MessageType.Error, ex.Message, "Could not open computer")))
                    .Subscribe()
                    .DisposeWith(disposables);
            });
		}



		public ReactiveCommand<string, ComputerObject> SetComputer { get; }

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
