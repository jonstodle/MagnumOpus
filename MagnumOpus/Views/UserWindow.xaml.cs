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
    public partial class UserWindow : DetailsWindow<UserWindowViewModel>
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
                d(this.Events().KeyDown
                    .Where(x => x.Key == System.Windows.Input.Key.F5)
                    .Select(_ => ViewModel.User.CN)
                    .InvokeCommand(ViewModel.SetUser));
                d(new List<Interaction<MessageInfo, int>>
                {
                    UserDetails.Messages,
                    UserAccountPanel.Messages,
                    UserProfilePanel.Messages,
                    UserGroups.Messages
                }.RegisterMessageHandler(ContainerGrid));
                d(new List<Interaction<DialogInfo, Unit>>
                {
                    UserAccountPanel.DialogRequests,
                    UserGroups.DialogRequests
                }.RegisterDialogHandler(ContainerGrid));
            });
        }
    }
}
