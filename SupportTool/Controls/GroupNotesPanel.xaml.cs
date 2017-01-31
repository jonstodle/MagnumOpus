using ReactiveUI;
using SupportTool.Models;
using SupportTool.ViewModels;
using System.Reactive;
using System.Windows;
using System.Windows.Controls;

namespace SupportTool.Controls
{
    /// <summary>
    /// Interaction logic for GroupNotesPanel.xaml
    /// </summary>
    public partial class GroupNotesPanel : UserControl, IViewFor<GroupNotesPanelViewModel>
	{
		public GroupNotesPanel()
		{
			InitializeComponent();

			ViewModel = new GroupNotesPanelViewModel();

			this.WhenActivated(d =>
			{
                d(this.Bind(ViewModel, vm => vm.Group, v => v.Group));

                d(this.Bind(ViewModel, vm => vm.Notes, v => v.NotesTextBox.Text));
                d(this.OneWayBind(ViewModel, vm => vm.IsEditingEnabled, v => v.NotesTextBox.IsEnabled));
                d(this.OneWayBind(ViewModel, vm => vm.IsEditingEnabled, v => v.EnableEditingButton.Visibility, x => x ? Visibility.Collapsed : Visibility.Visible));
                d(this.OneWayBind(ViewModel, vm => vm.IsEditingEnabled, v => v.SaveButton.Visibility));
                d(this.OneWayBind(ViewModel, vm => vm.IsEditingEnabled, v => v.CancelButton.Visibility));

                d(this.BindCommand(ViewModel, vm => vm.EnableEditing, v => v.EnableEditingButton));
				d(this.BindCommand(ViewModel, vm => vm.Save, v => v.SaveButton));
				d(this.BindCommand(ViewModel, vm => vm.Cancel, v => v.CancelButton));
			});
		}

		public Interaction<MessageInfo, Unit> InfoMessages => ViewModel.InfoMessages;

		public Interaction<MessageInfo, Unit> ErrorMessages => ViewModel.ErrorMessages;

		public Interaction<DialogInfo, Unit> DialogRequests => ViewModel.DialogRequests;

		public GroupObject Group
		{
			get { return (GroupObject)GetValue(GroupProperty); }
			set { SetValue(GroupProperty, value); }
		}

		public static readonly DependencyProperty GroupProperty = DependencyProperty.Register(nameof(Group), typeof(GroupObject), typeof(GroupNotesPanel), new PropertyMetadata(null));

		public GroupNotesPanelViewModel ViewModel
		{
			get { return (GroupNotesPanelViewModel)GetValue(ViewModelProperty); }
			set { SetValue(ViewModelProperty, value); }
		}

		public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(GroupNotesPanelViewModel), typeof(GroupNotesPanel), new PropertyMetadata(null));

		object IViewFor.ViewModel
		{
			get => ViewModel;
			set => ViewModel = value as GroupNotesPanelViewModel;
		}
	}
}
