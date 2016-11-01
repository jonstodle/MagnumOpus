using ReactiveUI;
using SupportTool.Services.NavigationServices;
using SupportTool.Services.SettingsServices;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace SupportTool.ViewModels
{
	public class SettingsWindowViewModel : ReactiveObject, IDialog
	{
		private readonly ReactiveCommand<Unit, Unit> _addHFName;
		private readonly ReactiveCommand<Unit, Unit> _removeHFName;
		private readonly ReactiveList<string> _remoteControl2012HFs = new ReactiveList<string>(SettingsService.Current.RemoteControl2012HFs);
		private readonly ListCollectionView _remoteControl2012HFsView;
		private string _historyCountLimit = SettingsService.Current.HistoryCountLimit.ToString();
		private string _detailWindowTimeoutLength = SettingsService.Current.DetailsWindowTimeoutLength.ToString();
		private string _hfName;
		private object _selectedRemoteControl2012HF;
		private Action _close;



		public SettingsWindowViewModel()
		{
			_remoteControl2012HFsView = new ListCollectionView(_remoteControl2012HFs)
			{
				SortDescriptions = { new SortDescription() }
			};

			_addHFName = ReactiveCommand.Create(
				() => _remoteControl2012HFs.Add(_hfName.Trim()),
				this.WhenAnyValue(x => x.HFName, x => x.HasValue(3)));
			_addHFName
				.Subscribe(_ => HFName = "");

			_removeHFName = ReactiveCommand.Create(
				() => { _remoteControl2012HFs.Remove((string)_selectedRemoteControl2012HF); },
				this.WhenAnyValue(x => x.SelectedRemoteControl2012HF).IsNotNull());

			this.WhenAnyValue(x => x.HistoryCountLimit)
				.Where(x => x.IsLong())
				.Select(x => int.Parse(x))
				.Where(x => x > 0)
				.Subscribe(x => SettingsService.Current.HistoryCountLimit = x);

			this.WhenAnyValue(x => x.DetailWindowTimeoutLength)
				.Where(x => x.IsLong())
				.Select(x => int.Parse(x))
				.Where(x => x > 0)
				.Subscribe(x => SettingsService.Current.DetailsWindowTimeoutLength = x);

			this.WhenAnyObservable(x => x._remoteControl2012HFs.Changed)
				.Select(x => _remoteControl2012HFs)
				.Subscribe(x => SettingsService.Current.RemoteControl2012HFs = x);
		}



		public ReactiveCommand AddHFName => _addHFName;

		public ReactiveCommand RemoveHFName => _removeHFName;

		public ReactiveList<string> RemoteControl2012HFs => _remoteControl2012HFs;

		public ListCollectionView RemoteControl2012HFsView => _remoteControl2012HFsView;

		public string HistoryCountLimit
		{
			get { return _historyCountLimit; }
			set { this.RaiseAndSetIfChanged(ref _historyCountLimit, value); }
		}

		public string DetailWindowTimeoutLength
		{
			get { return _detailWindowTimeoutLength; }
			set { this.RaiseAndSetIfChanged(ref _detailWindowTimeoutLength, value); }
		}

		public string HFName
		{
			get { return _hfName; }
			set { this.RaiseAndSetIfChanged(ref _hfName, value); }
		}

		public object SelectedRemoteControl2012HF
		{
			get { return _selectedRemoteControl2012HF; }
			set { this.RaiseAndSetIfChanged(ref _selectedRemoteControl2012HF, value); }
		}



		public Task Opening(Action close, object parameter)
		{
			_close = close;
			return Task.FromResult<object>(null);
		}
	}
}
