using ReactiveUI;
using SupportTool.Controls;
using SupportTool.Models;
using SupportTool.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows;

namespace SupportTool.Views
{
	/// <summary>
	/// Interaction logic for UserWindow.xaml
	/// </summary>
	public partial class UserWindow : DetailsWindow, IViewFor<UserWindowViewModel>
	{
		public UserWindow()
		{
			InitializeComponent();

			ViewModel = new UserWindowViewModel();

			this.OneWayBind(ViewModel, vm => vm.User.Principal.Name, v => v.Title, x => x ?? "");
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
				d(new List<Interaction<MessageInfo, Unit>>
				{
					UserDetails.InfoMessages,
					UserAccountPanel.InfoMessages,
					UserProfilePanel.InfoMessages,
					UserGroups.InfoMessages
				}.RegisterInfoHandler(ContainerGrid));
				d(new List<Interaction<MessageInfo, Unit>>
				{
					UserDetails.ErrorMessages,
					UserAccountPanel.ErrorMessages,
					UserProfilePanel.ErrorMessages,
					UserGroups.ErrorMessages
				}.RegisterErrorHandler(ContainerGrid));
				d(new List<Interaction<DialogInfo, Unit>>
				{
					UserAccountPanel.DialogRequests,
					UserGroups.DialogRequests
				}.RegisterDialogHandler(ContainerGrid));
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
