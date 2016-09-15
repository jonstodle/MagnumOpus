using ReactiveUI;
using SupportTool.ViewModels;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SupportTool.Views
{
	/// <summary>
	/// Interaction logic for RemoveGroupsWindow.xaml
	/// </summary>
	public partial class RemoveGroupsWindow : Window, IViewFor<RemoveGroupsWindowViewModel>
    {
        public RemoveGroupsWindow()
        {
            InitializeComponent();

            this.OneWayBind(ViewModel, vm => vm.WindowTitle, v => v.Title, x => x ?? "");
            this.OneWayBind(ViewModel, vm => vm.PrincipalGroupsView, v => v.PrincipalGroupsListView.ItemsSource);
            this.Bind(ViewModel, vm => vm.SelectedPrincipalGroup, v => v.PrincipalGroupsListView.SelectedItem);
            this.OneWayBind(ViewModel, vm => vm.GroupsToRemoveView, v => v.GroupsToRemoveListView.ItemsSource);
            this.Bind(ViewModel, vm => vm.SelectedGroupToRemove, v => v.GroupsToRemoveListView.SelectedItem);

            this.WhenActivated(d =>
            {
                d(PrincipalGroupsListView.Events()
                    .MouseDoubleClick
                    .Select(_ => Unit.Default)
                    .InvokeCommand(ViewModel, x => x.AddGroupToGroupsToRemove));
                d(this.BindCommand(ViewModel, vm => vm.AddGroupToGroupsToRemove, v => v.AddToGroupsToRemoveButton));
                d(GroupsToRemoveListView.Events()
                    .MouseDoubleClick
                    .Select(_ => Unit.Default)
                    .InvokeCommand(ViewModel, x => x.RemoveGroupFromGroupsToRemove));
                d(this.BindCommand(ViewModel, vm => vm.RemoveGroupFromGroupsToRemove, v => v.RemoveFromGroupsToRemoveButton));
                d(this.BindCommand(ViewModel, vm => vm.RemovePrincipalFromGroups, v => v.RemovePrincipalFromGroupsButton));
            });
        }

        public RemoveGroupsWindowViewModel ViewModel
        {
            get { return (RemoveGroupsWindowViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(RemoveGroupsWindowViewModel), typeof(RemoveGroupsWindow), new PropertyMetadata(new RemoveGroupsWindowViewModel()));

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = value as RemoveGroupsWindowViewModel; }
        }
    }
}
