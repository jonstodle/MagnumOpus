using ReactiveUI;
using MagnumOpus.Controls;
using MagnumOpus.Models;
using MagnumOpus.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows;

namespace MagnumOpus.Views
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

            this.WhenActivated(d =>
            {
                d(this.OneWayBind(ViewModel, vm => vm.User.Name, v => v.Title, x => x ?? ""));
                d(this.OneWayBind(ViewModel, vm => vm.User, v => v.UserDetails.User));
                d(this.OneWayBind(ViewModel, vm => vm.User, v => v.UserAccountPanel.User));
                d(this.OneWayBind(ViewModel, vm => vm.User, v => v.UserProfilePanel.User));
                d(this.OneWayBind(ViewModel, vm => vm.User, v => v.UserGroups.User));

                d(MessageBus.Current.Listen<string>(ApplicationActionRequest.Refresh)
                    .ObserveOnDispatcher()
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

        public UserWindowViewModel ViewModel { get => (UserWindowViewModel)GetValue(ViewModelProperty); set => SetValue(ViewModelProperty, value); }
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(UserWindowViewModel), typeof(UserWindow), new PropertyMetadata(null));

        object IViewFor.ViewModel { get => ViewModel; set => ViewModel = value as UserWindowViewModel; }
    }
}
