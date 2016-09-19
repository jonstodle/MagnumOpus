﻿using ReactiveUI;
using SupportTool.Models;
using SupportTool.Services.ActiveDirectoryServices;
using SupportTool.Services.NavigationServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupportTool.ViewModels
{
	public class ComputerWindowViewModel : ReactiveObject, INavigable
	{
		private readonly ReactiveCommand<string, ComputerObject> _setComputer;
		private readonly ObservableAsPropertyHelper<ComputerObject> _computer;



		public ComputerWindowViewModel()
		{
			_setComputer = ReactiveCommand.CreateFromObservable<string, ComputerObject>(identity => ActiveDirectoryService.Current.GetComputer(identity));
			_setComputer
				.ToProperty(this, x => x.Computer, out _computer);
		}



		public ReactiveCommand SetComputer => _setComputer;

		public ComputerObject Computer => _computer.Value;



		public Task OnNavigatedTo(object parameter)
		{
			if (parameter is string)
			{
				Observable.Return(parameter as string)
					.InvokeCommand(_setComputer);
			}

			return Task.FromResult<object>(null);
		}

		public Task OnNavigatingFrom() => Task.FromResult<object>(null);
	}
}
