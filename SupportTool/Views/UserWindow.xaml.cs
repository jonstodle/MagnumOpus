﻿using ReactiveUI;
using SupportTool.Models;
using SupportTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
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

			ViewModel = new UserWindowViewModel();

			this.OneWayBind(ViewModel, vm => vm.User.CN, v => v.Title, x => x ?? "");
			this.OneWayBind(ViewModel, vm => vm.User, v => v.UserDetails.User);
			this.OneWayBind(ViewModel, vm => vm.User, v => v.UserAccountPanel.User);
			this.OneWayBind(ViewModel, vm => vm.User, v => v.UserProfilePanel.User);
			this.OneWayBind(ViewModel, vm => vm.User, v => v.UserGroups.User);

			this.WhenActivated(d =>
			{
				d(MessageBus.Current.Listen<string>(ApplicationActionRequest.Refresh.ToString())
					.Where(x => x == ViewModel.User?.CN)
					.InvokeCommand(ViewModel, x => x.SetUser));
				d(this.BindCommand(ViewModel, vm => vm.SetUser, v => v.RefreshHyperLink, ViewModel.WhenAnyValue(x => x.User.CN)));
			});
		}

		public UserWindowViewModel ViewModel
		{
			get { return (UserWindowViewModel)GetValue(ViewModelProperty); }
			set { SetValue(ViewModelProperty, value); }
		}

		public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(UserWindowViewModel), typeof(UserWindow), new PropertyMetadata(null));

		object IViewFor.ViewModel
		{
			get { return ViewModel; }
			set { ViewModel = value as UserWindowViewModel; }
		}
	}
}
