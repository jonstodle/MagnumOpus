using ReactiveUI;
using SupportTool.Models;
using SupportTool.Services.ActiveDirectoryServices;
using SupportTool.Services.NavigationServices;
using System;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace SupportTool.ViewModels
{
	public class GroupWindowViewModel : ViewModelBase, INavigable
	{
		private readonly ReactiveCommand<string, GroupObject> _setGroup;
		private readonly ObservableAsPropertyHelper<GroupObject> _group;



		public GroupWindowViewModel()
		{
			_setGroup = ReactiveCommand.CreateFromObservable<string, GroupObject>(identity => ActiveDirectoryService.Current.GetGroup(identity));
			_setGroup
				.ToProperty(this, x => x.Group, out _group);
			_setGroup
				.ThrownExceptions
				.Subscribe(async ex => await _errorMessages.Handle(new MessageInfo(ex.Message)));
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
	}
}
