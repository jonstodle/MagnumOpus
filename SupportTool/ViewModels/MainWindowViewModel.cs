using ReactiveUI;
using SupportTool.Helpers;
using SupportTool.Models;
using SupportTool.Services.ActiveDirectoryServices;
using SupportTool.Services.DialogServices;
using SupportTool.Services.NavigationServices;
using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace SupportTool.ViewModels
{
	public partial class MainWindowViewModel : ReactiveObject, INavigable
	{
		private readonly ReactiveCommand<Unit, Principal> _find;
		private readonly ReactiveCommand<Unit, bool> _search;
		private readonly ReactiveCommand<Unit, Unit> _pasteAndSearch;
		private readonly ReactiveCommand<Unit, Unit> _navigateBack;
		private readonly ReactiveCommand<Unit, Unit> _navigateForward;
		private readonly ReactiveList<string> _history;
		private ObservableAsPropertyHelper<List<string>> _reverseHistory;
		private UserObject _user;
		private ComputerObject _computer;
		private GroupObject _group;
		private string _ipAddress;
		private string _queryString;
		private int _backwardStepsCount;



		public MainWindowViewModel()
		{
			_history = new ReactiveList<string>();
			_backwardStepsCount = 0;

			MessageBus.Current.Listen<ApplicationActionRequest>()
				.Subscribe(a => ApplicationActionRequestImpl(a));

			_find = ReactiveCommand.CreateFromObservable(
				() =>
				{
					return ActiveDirectoryService.Current.GetPrincipal(_queryString)
					.Select(x =>
					{
						if (x != null) return x;
						throw new Exception("Principal not found");
					})
					.SubscribeOn(RxApp.TaskpoolScheduler)
					.ObserveOnDispatcher()
					.Catch(Observable.FromAsync(() => NavigationService.ShowDialog<Views.SearchWindow, Principal>(_queryString)));
				},
				this.WhenAnyValue(x => x.QueryString, x => x.HasValue()));
			_find
				.NotNull()
				.ObserveOnDispatcher()
				.Subscribe(x =>
				{
					NullValues();

					if (x is UserPrincipal) User = new UserObject(x as UserPrincipal);
					else if (x is ComputerPrincipal) Computer = new ComputerObject(x as ComputerPrincipal);
					else if (x is GroupPrincipal) Group = new GroupObject(x as GroupPrincipal);
				});

			_search = ReactiveCommand.Create(
				() => Regex.IsMatch(_queryString, @"^(?:[0-9]{1,3}\.){3}[0-9]{1,3}$"),
				this.WhenAnyValue(x => x.QueryString, x => x.HasValue(3)));
			_search
				.Where(x => x != true)
				.Select(_ => Unit.Default)
				.InvokeCommand(_find);
			_search
				.Where(x => x == true)
				.Subscribe(_ =>
				{
					NullValues();
					IPAddress = _queryString;
				});

			_pasteAndSearch = ReactiveCommand.Create(() => { QueryString = Clipboard.GetText()?.ToUpperInvariant(); });
			_pasteAndSearch
				.InvokeCommand(_search);

			_navigateBack = ReactiveCommand.Create(
				() =>
				{
					BackwardStepsCount += 1;
					QueryString = _history.Reverse().Skip(_backwardStepsCount).FirstOrDefault();
				},
				Observable.CombineLatest(
					this.WhenAnyValue(x => x.BackwardStepsCount),
					this.WhenAnyObservable(y => y._history.CountChanged),
					(x, y) => x < (y - 1)));

			_navigateForward = ReactiveCommand.Create(
				() =>
				{
					BackwardStepsCount -= 1;
					QueryString = _history.Reverse().Skip(_backwardStepsCount).FirstOrDefault();
				},
				this.WhenAnyValue(x => x.BackwardStepsCount, x => x > 0));

			Observable.Merge(
				_navigateBack,
				_navigateForward)
				.InvokeCommand(_search);

			_reverseHistory = this
				.WhenAnyObservable(x => x._history.Changed)
				.Select(_ => _history.Reverse().ToList())
				.ToProperty(this, x => x.ReverseHistory, new List<string>());

			var principalChanged = Observable.Merge(
				this.WhenAnyValue(x => x.User.CN).NotNull(),
				this.WhenAnyValue(x => x.Computer.CN).NotNull(),
				this.WhenAnyValue(x => x.Group.CN).NotNull(),
				this.WhenAnyValue(x => x.IPAddress));
			principalChanged
				.Where(x => x.HasValue(3))
				.Where(x => !_history.Contains(x))
				.Subscribe(x =>
				{
					BackwardStepsCount = 0;
					_history.Add(x);
				});
			principalChanged
				.Where(x => _history.Contains(x))
				.Subscribe(x =>
				{
					BackwardStepsCount = _reverseHistory.Value.IndexOf(x);
				});


			Observable.Merge(
				_navigateBack.ThrownExceptions,
				_navigateForward.ThrownExceptions,
				_pasteAndSearch.ThrownExceptions,
				_find.ThrownExceptions)
				.Subscribe(ex => DialogService.ShowError(ex.Message));
		}



		public ReactiveCommand Find => _find;

		public ReactiveCommand Search => _search;

		public ReactiveCommand PasteAndSearch => _pasteAndSearch;

		public ReactiveCommand NavigateBack => _navigateBack;

		public ReactiveCommand NavigateForward => _navigateForward;

		public ReactiveList<string> History => _history;

		public List<string> ReverseHistory => _reverseHistory.Value;

		public UserObject User
		{
			get { return _user; }
			set { this.RaiseAndSetIfChanged(ref _user, value); }
		}

		public ComputerObject Computer
		{
			get { return _computer; }
			set { this.RaiseAndSetIfChanged(ref _computer, value); }
		}

		public GroupObject Group
		{
			get { return _group; }
			set { this.RaiseAndSetIfChanged(ref _group, value); }
		}

		public string IPAddress
		{
			get { return _ipAddress; }
			set { this.RaiseAndSetIfChanged(ref _ipAddress, value); }
		}

		public string QueryString
		{
			get { return _queryString; }
			set { this.RaiseAndSetIfChanged(ref _queryString, value); }
		}

		public int BackwardStepsCount
		{
			get { return _backwardStepsCount; }
			set { this.RaiseAndSetIfChanged(ref _backwardStepsCount, value); }
		}



		private void NullValues()
		{
			User = null;
			Group = null;
			Computer = null;
			IPAddress = null;
		}

		private async void ApplicationActionRequestImpl(ApplicationActionRequest a)
		{
			switch (a)
			{
				case ApplicationActionRequest.Refresh:
					await _find.Execute();
					break;
				default:
					break;
			}
		}

		private void AddToPreviousIdentities(string item)
		{
			if (item != (_history.LastOrDefault() ?? "")) _history.Add(item);
		}



		public Task OnNavigatedTo(object parameter) => Task.FromResult<object>(null);

		public Task OnNavigatingFrom() => Task.FromResult<object>(null);
	}
}
