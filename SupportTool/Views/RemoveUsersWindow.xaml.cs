using ReactiveUI;
using SupportTool.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace SupportTool.Views
{
	/// <summary>
	/// Interaction logic for RemoveUsersWindow.xaml
	/// </summary>
	public partial class RemoveUsersWindow : Window, IViewFor<RemoveUsersWindowViewModel>
	{
		public RemoveUsersWindow()
		{
			InitializeComponent();

			ViewModel = new RemoveUsersWindowViewModel();

			this.OneWayBind(ViewModel, vm => vm.WindowTitle, v => v.Title);

			this.OneWayBind(ViewModel, vm => vm.MembersView, v => v.MembersListView.ItemsSource);
			this.Bind(ViewModel, vm => vm.SelectedMember, v => v.MembersListView.SelectedItem);
			this.OneWayBind(ViewModel, vm => vm.MembersToRemoveView, v => v.MembersToRemoveListView.ItemsSource);
			this.Bind(ViewModel, vm => vm.SelectedMemberToRemove, v => v.MembersToRemoveListView.SelectedItem);

			this.WhenActivated(d =>
			{
				d(this.BindCommand(ViewModel, vm => vm.AddToMembersToRemove, v => v.MembersListView, nameof(ListView.MouseDoubleClick)));
				d(this.BindCommand(ViewModel, vm => vm.AddToMembersToRemove, v => v.AddToMembersToRemoveButton));
				d(this.BindCommand(ViewModel, vm => vm.RemoveFromMembersToRemove, v => v.MembersToRemoveListView, nameof(ListView.MouseDoubleClick)));
				d(this.BindCommand(ViewModel, vm => vm.RemoveFromMembersToRemove, v => v.RemoveFromMembersToRemoveButton));
				d(this.BindCommand(ViewModel, vm => vm.Save, v => v.SaveButton));
			});
		}

		public RemoveUsersWindowViewModel ViewModel
		{
			get { return (RemoveUsersWindowViewModel)GetValue(ViewModelProperty); }
			set { SetValue(ViewModelProperty, value); }
		}

		public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(RemoveUsersWindowViewModel), typeof(RemoveUsersWindow), new PropertyMetadata(null));

		object IViewFor.ViewModel
		{
			get { return ViewModel; }
			set { ViewModel = value as RemoveUsersWindowViewModel; }
		}
	}
}
