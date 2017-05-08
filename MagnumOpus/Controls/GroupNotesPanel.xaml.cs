using ReactiveUI;
using MagnumOpus.Models;
using MagnumOpus.ViewModels;
using System.Reactive;
using System.Windows;
using System.Windows.Controls;
using System.Reactive.Disposables;

namespace MagnumOpus.Controls
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
                this.Bind(ViewModel, vm => vm.Group, v => v.Group).DisposeWith(d);

                this.Bind(ViewModel, vm => vm.Notes, v => v.NotesTextBox.Text).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.IsEditingEnabled, v => v.NotesTextBox.IsEnabled).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.IsEditingEnabled, v => v.EnableEditingButton.Visibility, x => x ? Visibility.Collapsed : Visibility.Visible).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.IsEditingEnabled, v => v.SaveButton.Visibility).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.IsEditingEnabled, v => v.CancelButton.Visibility).DisposeWith(d);

                this.BindCommand(ViewModel, vm => vm.EnableEditing, v => v.EnableEditingButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.Save, v => v.SaveButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.Cancel, v => v.CancelButton).DisposeWith(d);
            });
        }

        public Interaction<MessageInfo, int> Messages => ViewModel.Messages;

        public Interaction<DialogInfo, Unit> DialogRequests => ViewModel.DialogRequests;

        public GroupObject Group { get => (GroupObject)GetValue(GroupProperty); set => SetValue(GroupProperty, value); }
        public static readonly DependencyProperty GroupProperty = DependencyProperty.Register(nameof(Group), typeof(GroupObject), typeof(GroupNotesPanel), new PropertyMetadata(null));

        public GroupNotesPanelViewModel ViewModel { get => (GroupNotesPanelViewModel)GetValue(ViewModelProperty); set => SetValue(ViewModelProperty, value); }
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(GroupNotesPanelViewModel), typeof(GroupNotesPanel), new PropertyMetadata(null));

        object IViewFor.ViewModel { get => ViewModel; set => ViewModel = value as GroupNotesPanelViewModel; }
    }
}
