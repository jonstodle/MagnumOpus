using ReactiveUI;
using SupportTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SupportTool.Views
{
	/// <summary>
	/// Interaction logic for UserWindow.xaml
	/// </summary>
	public partial class UserWindow : Window, IViewFor<UserWindowViewModel>
	{
		public UserWindow()
		{
			InitializeComponent();

			this.OneWayBind(ViewModel, vm => vm.User.CN, v => v.Title, x => x ?? "");
			this.OneWayBind(ViewModel, vm => vm.User, v => v.UserDetails.User);
			this.OneWayBind(ViewModel, vm => vm.User, v => v.UserAccountPanel.User);
			this.OneWayBind(ViewModel, vm => vm.User, v => v.UserProfilePanel.User);
			this.OneWayBind(ViewModel, vm => vm.User, v => v.UserGroups.User);
		}

		public UserWindowViewModel ViewModel
		{
			get { return (UserWindowViewModel)GetValue(ViewModelProperty); }
			set { SetValue(ViewModelProperty, value); }
		}

		public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(UserWindowViewModel), typeof(UserWindow), new PropertyMetadata(new UserWindowViewModel()));

		object IViewFor.ViewModel
		{
			get { return ViewModel; }
			set { ViewModel = value as UserWindowViewModel; }
		}
	}
}
