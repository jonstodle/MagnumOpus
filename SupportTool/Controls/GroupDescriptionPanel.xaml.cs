using ReactiveUI;
using SupportTool.Models;
using SupportTool.ViewModels;
using System.Reactive;
using System.Windows;
using System.Windows.Controls;

namespace SupportTool.Controls
{
    /// <summary>
    /// Interaction logic for GroupDescriptionPanel.xaml
    /// </summary>
    public partial class GroupDescriptionPanel : UserControl, IViewFor<GroupDescriptionPanelViewModel>
    {
        public GroupDescriptionPanel()
        {
            InitializeComponent();

            ViewModel = new GroupDescriptionPanelViewModel();

            this.WhenActivated(d =>
            {
                d(this.Bind(ViewModel, vm => vm.Group, v => v.Group));

                d(this.Bind(ViewModel, vm => vm.Description, v => v.DescriptionTextBox.Text));
                d(this.OneWayBind(ViewModel, vm => vm.HasDescriptionChanged, v => v.DescriptionButtonsStackPanel.Visibility));

                d(this.BindCommand(ViewModel, vm => vm.Save, v => v.SaveButton));
                d(this.BindCommand(ViewModel, vm => vm.Cancel, v => v.CancelButton));
            });
        }

        public Interaction<MessageInfo, Unit> InfoMessages => ViewModel.InfoMessages;

        public Interaction<MessageInfo, Unit> ErrorMessages => ViewModel.ErrorMessages;

        public Interaction<DialogInfo, Unit> DialogRequests => ViewModel.DialogRequests;

        public GroupObject Group { get => (GroupObject)GetValue(GroupProperty); set => SetValue(GroupProperty, value); }
        public static readonly DependencyProperty GroupProperty = DependencyProperty.Register(nameof(Group), typeof(GroupObject), typeof(GroupDescriptionPanel), new PropertyMetadata(null));

        public GroupDescriptionPanelViewModel ViewModel { get => (GroupDescriptionPanelViewModel)GetValue(ViewModelProperty); set => SetValue(ViewModelProperty, value); }
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(GroupDescriptionPanelViewModel), typeof(GroupDescriptionPanel), new PropertyMetadata(null));

        object IViewFor.ViewModel { get => ViewModel; set => ViewModel = value as GroupDescriptionPanelViewModel; }
    }
}
