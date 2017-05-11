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
			SetGroup = ReactiveCommand.CreateFromObservable<string, GroupObject>(identity => ActiveDirectoryService.Current.GetGroup(identity));

            _group = SetGroup
                .ToProperty(this, x => x.Group);

            (this).WhenActivated((Action<CompositeDisposable>)(disposables =>
            {
                Observable.SelectMany<Exception, int>(this.SetGroup
                    .ThrownExceptions
, (Func<Exception, IObservable<int>>)(ex => (IObservable<int>)_messages.Handle((MessageInfo)new MessageInfo((MessageType)MessageType.Error, (string)ex.Message, (string)"Could not load group"))))
                    .Subscribe()
                    .DisposeWith(disposables);
            }));
		}



		public ReactiveCommand<string, GroupObject> SetGroup { get; private set; }
        public GroupObject Group => _group.Value;



		public Task OnNavigatedTo(object parameter)
		{
			if (parameter is string s)
			{
				Observable.Return(s)
					.InvokeCommand(SetGroup);
			}

			return Task.FromResult<object>(null);
		}

		public Task OnNavigatingFrom() => Task.FromResult<object>(null);



        private readonly ObservableAsPropertyHelper<GroupObject> _group;
    }
}
