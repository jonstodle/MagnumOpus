using ReactiveUI;
using SupportTool.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupportTool.ViewModels
{
	public class GroupDescriptionPanelViewModel : ViewModelBase
	{
		public GroupDescriptionPanelViewModel()
		{
			_save = ReactiveCommand.CreateFromObservable(() => SaveImpl(_group, _description));
			_save
				.Subscribe(_ =>
				{
					_descriptionBackup = _description;
					this.RaisePropertyChanged(nameof(_descriptionBackup));
				});

			_cancel = ReactiveCommand.Create(() => { Description = _descriptionBackup; });

			_isDescriptionDirty = this.WhenAnyValue(
				x => x.Description,
				y => y._descriptionBackup,
				(x, y) => (x ?? "") != (y ?? ""))
				.ToProperty(this, x => x.IsDescriptionDirty);

			this.WhenAnyValue(x => x.Group)
				.WhereNotNull()
				.Select(x => x.Principal.Description)
				.Subscribe(x =>
				{
					_descriptionBackup = x;
					Description = _descriptionBackup;
				});

			Observable.Merge(
				_save.ThrownExceptions,
				_cancel.ThrownExceptions)
				.Subscribe(async ex => await _errorMessages.Handle(new MessageInfo(ex.Message)));
		}



		public ReactiveCommand Save => _save;

		public ReactiveCommand Cancel => _cancel;

		public bool IsDescriptionDirty => _isDescriptionDirty.Value;

		public GroupObject Group
		{
			get { return _group; }
			set { this.RaiseAndSetIfChanged(ref _group, value); }
		}

		public string Description
		{
			get { return _description; }
			set { this.RaiseAndSetIfChanged(ref _description, value); }
		}



		private IObservable<Unit> SaveImpl(GroupObject group, string description) => Observable.Start(() =>
		{
			group.Principal.Description = description.HasValue() ? description : null;
			group.Principal.Save();
		});



		private readonly ReactiveCommand<Unit, Unit> _save;
		private readonly ReactiveCommand<Unit, Unit> _cancel;
		private readonly ObservableAsPropertyHelper<bool> _isDescriptionDirty;
		private GroupObject _group;
		private string _description;
		private string _descriptionBackup;
	}
}
