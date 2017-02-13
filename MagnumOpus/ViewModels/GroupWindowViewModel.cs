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
	public class GroupWindowViewModel : ViewModelBase, INavigable
	{
		public GroupWindowViewModel()
		{
			_setGroup = ReactiveCommand.CreateFromObservable<string, GroupObject>(identity => ActiveDirectoryService.Current.GetGroup(identity));

            _group = _setGroup
                .ToProperty(this, x => x.Group);

            this.WhenActivated(disposables =>
            {
                _setGroup
                    .ThrownExceptions
                    .Subscribe(async ex => await _errorMessages.Handle(new MessageInfo(ex.Message)))
                    .DisposeWith(disposables);
            });
		}



		public ReactiveCommand SetGroup => _setGroup;

		public GroupObject Group => _group.Value;



		public Task OnNavigatedTo(object parameter)
		{
			if (parameter is string)
			{
				Observable.Return(parameter as string)
					.InvokeCommand(_setGroup);
			}

			return Task.FromResult<object>(null);
		}

		public Task OnNavigatingFrom() => Task.FromResult<object>(null);



        private readonly ReactiveCommand<string, GroupObject> _setGroup;
        private readonly ObservableAsPropertyHelper<GroupObject> _group;
    }
}
