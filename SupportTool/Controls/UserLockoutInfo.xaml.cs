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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SupportTool.Controls
{
	/// <summary>
	/// Interaction logic for UserLockoutInfo.xaml
	/// </summary>
	public partial class UserLockoutInfo : UserControl, IViewFor<UserLockoutInfoViewModel>
	{
		public UserLockoutInfo()
		{
			InitializeComponent();

			this.WhenActivated(d =>
			{
                d(this.OneWayBind(ViewModel, vm => vm.User.Name, v => v.TitleTextBlock.Text, x => $"Lockout info for {x}"));
                d(this.OneWayBind(ViewModel, vm => vm.LockoutInfos, v => v.LockoutInfosListView.ItemsSource));

                d(this.BindCommand(ViewModel, vm => vm.GetLockoutInfo, v => v.RefreshButton));
				d(this.BindCommand(ViewModel, vm => vm.Close, v => v.CloseButton));
			});
		}

		public UserLockoutInfoViewModel ViewModel
		{
			get { return (UserLockoutInfoViewModel)GetValue(ViewModelProperty); }
			set { SetValue(ViewModelProperty, value); }
		}

		public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(UserLockoutInfoViewModel), typeof(UserLockoutInfo), new PropertyMetadata(null));

		object IViewFor.ViewModel
		{
			get => ViewModel;
			set => ViewModel = value as UserLockoutInfoViewModel;
		}
	}
}
