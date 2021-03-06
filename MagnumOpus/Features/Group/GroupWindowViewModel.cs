﻿using ReactiveUI;
using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using MagnumOpus.ActiveDirectory;
using MagnumOpus.Dialog;
using MagnumOpus.Navigation;
using Splat;

namespace MagnumOpus.Group
{
	public class GroupWindowViewModel : ViewModelBase, INavigable
	{
		public GroupWindowViewModel()
		{
			SetGroup = ReactiveCommand.CreateFromObservable<string, GroupObject>(identity => Locator.Current.GetService<ADFacade>().GetGroup(identity));

            _group = SetGroup
                .ToProperty(this, vm => vm.Group);

            this.WhenActivated(disposables =>
            {
                SetGroup.ThrownExceptions
                    .SelectMany(ex => _messages.Handle(new MessageInfo(MessageType.Error, ex.Message, "Could not load group")))
                    .Subscribe()
                    .DisposeWith(disposables);
            });
		}



		public ReactiveCommand<string, GroupObject> SetGroup { get; }
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
